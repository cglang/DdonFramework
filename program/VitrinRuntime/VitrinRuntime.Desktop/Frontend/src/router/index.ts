import { createRouter, createWebHistory } from 'vue-router'
import MainView from '../views/MainView.vue'
import PlcListView from '../views/PlcListView.vue'
import PlcDetailView from '../views/PlcDetailView.vue'

const routes = [
  { path: '/', redirect: '/main' },
  { path: '/main', name: 'Main', component: MainView },
  { path: '/plc/list', name: 'PlcList', component: PlcListView },
  { path: '/plc/detail/:name', name: 'PlcDetail', component: PlcDetailView, props: true },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  return true
})

export default router
