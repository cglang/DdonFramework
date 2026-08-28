import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/window'
    },
    {
      path: '/naiveui',
      name: 'naiveui',
      component: () => import('../views/NaiveUI.vue'),
    },
    {
      path: '/window',
      name: 'window',
      component: () => import('../Window.vue'),
      children: [
        {
          path: 'wb',          // 实际路径为 /parent/child
          component: () => import('../views/WorkbenchView.vue'),
        },
        {
          path: 'naiveui',
          name: 'naiveui',
          component: () => import('../views/NaiveUI.vue'),
        },
      ]
    },
    {
      path: '/wl',
      name: 'winformlayout',
      component: () => import('../views/WinformLayout.vue'),
    }
  ],
})

export default router
