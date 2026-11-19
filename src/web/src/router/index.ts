import { createRouter, createWebHistory, type RouteRecordRaw, type RouterHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'

const developmentRoutes: RouteRecordRaw[] = import.meta.env.DEV
  ? [{ path: '/ui-sample', name: 'ui-sample', component: () => import('@/views/UiSampleView.vue') }]
  : []

export function createAppRouter(history: RouterHistory = createWebHistory()) {
  return createRouter({
    history,
    routes: [{ path: '/', name: 'home', component: HomeView }, ...developmentRoutes],
  })
}
