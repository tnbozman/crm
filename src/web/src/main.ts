import { createApp } from 'vue'
import { createPinia } from 'pinia'
import OpenVue from 'openvue/config'
import App from './App.vue'
import { createAppRouter } from './router'
import { openVueOptions } from './ui/openvue'
import './assets/main.css'

createApp(App).use(createPinia()).use(createAppRouter()).use(OpenVue, openVueOptions).mount('#app')
