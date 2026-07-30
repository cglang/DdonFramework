import { createRouter, createWebHistory } from 'vue-router'
import MainView from '../views/MainView.vue'
import PlcListView from '../views/PlcListView.vue'
import PlcDetailView from '../views/PlcDetailView.vue'
import LuaEngineView from '../views/LuaEngineView.vue'
import OpcUaServerView from '../views/OpcUaServerView.vue'

const routes = [
  { path: '/', redirect: '/main' },
  { path: '/main', name: 'Main', component: MainView },
  { path: '/plc/list', name: 'PlcList', component: PlcListView, meta: { location: 'PLC管理' } },
  { path: '/plc/detail/:name', name: 'PlcDetail', component: PlcDetailView, props: true, meta: { location: 'PLC详情' } },
  { path: '/lua/engine', name: 'LuaEngine', component: LuaEngineView, meta: { location: 'Lua脚本引擎' } },
  { path: '/opcua/server', name: 'OpcUaServer', component: OpcUaServerView, meta: { location: 'OPC UA Server' } },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  return true
})

export default router
