declare global {
  interface Window {
    ui: UiBridge,
    platform?: string
  }
}

interface Request {
  id: string;
  method: string;
  payload?: unknown;
}

interface Response {
  id: string;
  success: boolean;
  data?: unknown;
  error?: string;
}

interface Transport {
  invoke<T>(method: string, payload?: unknown): Promise<T>;
  publish(eventName: string, data?: unknown): Promise<void>;
  on<T>(eventName: string, handler: (data: T) => void): void;
  off(eventName: string): void;
  onMessage(message: string): void;
  sendMessage(message: string): void;
}

class BrowserTransport implements Transport {
  private baseUrl: string;
  constructor(baseUrl = '') {
    this.baseUrl = baseUrl;
  }

  async invoke<T>(method: string, payload?: unknown): Promise<T> {
    const req: Request = { id: crypto.randomUUID(), method, payload };
    const r = await fetch(`${this.baseUrl}/api/bridge/invoke`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(req) });
    const res: Response = await r.json();
    if (!res.success) throw new Error(res.error ?? 'Bridge invoke failed');
    return res.data as T;
  }

  async publish(eventName: string, data?: unknown): Promise<void> {
    await fetch(`${this.baseUrl}/api/bridge/event`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name: eventName, data }) });
  }

  on<T>(_eventName: string, _handler: (data: T) => void): void { }

  off(_eventName: string): void { }

  onMessage(_message: string): void { }

  sendMessage(_message: string): void { }
}

class WebViewTransport implements Transport {
  private pending = new Map<string, (r: Response) => void>();
  private handlers = new Map<string, (d: unknown) => void>();

  onMessage(_message: string): void {
    const msg = JSON.parse(_message);
    if (msg.type === 'response') {
      const resolve = this.pending.get(msg.data.id);
      if (resolve) {
        resolve(msg.data);
        this.pending.delete(msg.data.id);
      }
    } else if (msg.type === 'event') {
      const h = this.handlers.get(msg.data.name);
      if (h) h(msg.data.data);
    }
  }

  invoke<T>(method: string, payload?: unknown): Promise<T> {
    return new Promise((resolve, reject) => {
      const req: Request = { id: crypto.randomUUID(), method, payload };
      this.pending.set(req.id, (r) => {
        if (!r.success) reject(new Error(r.error ?? 'Bridge invoke failed'));
        else resolve(r.data as T);
      });
      invokeCSharpAction(JSON.stringify({ type: 'invoke', data: req }));
    });
  }

  async publish(eventName: string, data?: unknown): Promise<void> {
    invokeCSharpAction(JSON.stringify({ type: 'event', data: { name: eventName, data } }));
  }

  on<T>(eventName: string, handler: (data: T) => void): void {
    this.handlers.set(eventName, handler as (d: unknown) => void);
  }

  off(eventName: string): void {
    this.handlers.delete(eventName);
  }

  sendMessage(_message: string): void {
    invokeCSharpAction(_message)
  }
}

export interface UiBridge {
  invoke<T>(method: string, payload?: unknown): Promise<T>;
  publish(eventName: string, data?: unknown): Promise<void>;
  on<T>(eventName: string, handler: (data: T) => void): void;
  off(eventName: string): void;
  onMessage(message: string): void;
  seedMessage(message: string): void;
}

export function createBridge(): UiBridge {
  const isWebView = window.platform == "webview";
  const transport = isWebView ? new WebViewTransport() : new BrowserTransport("http://localhost:5000");
  return {
    invoke: <T>(m: string, p?: unknown) => transport.invoke<T>(m, p),
    publish: (e: string, d?: unknown) => transport.publish(e, d),
    on: <T>(e: string, h: (d: T) => void) => transport.on(e, h),
    off: (e: string) => transport.off(e),
    onMessage: (e: string) => transport.onMessage(e),
    seedMessage: (e: string) => transport.sendMessage(e),
  };
}

declare global {
  function invokeCSharpAction(data: unknown): void;
  function injectBridge(): void;
}

globalThis.injectBridge = () => {
  window.platform = "webview"
  init();
};

export function init() {
  window.ui = createBridge()
}
