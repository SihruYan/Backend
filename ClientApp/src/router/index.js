// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router';
import LoginView from "../components/LoginView.vue";
import DashboardLayout from "../components/DashboardLayout.vue";
import DashboardHome from "../components/DashboardHome.vue";
import FormDetail from "../components/FormDetail.vue";
import BlogEditor from "../components/BlogEditor.vue";
import NewsList from "../components/NewsList.vue";
import BlogList from "../components/BlogList.vue";
import NewsEditor from "../components/NewsEditor.vue";

function FormList() {
    
}

const routes = [
    {
        path: '/',
        component: LoginView
    },
    {
        path: '/dashboard',
        component: DashboardLayout,
        meta: { requiresAuth: true },
        children: [
            {
                path: '',
                name: 'Dashboard',
                component: DashboardHome,
                meta: { requiresAuth: true }
            },
            {
                path: 'forms',
                name: 'FormList',
                component: FormList,
                meta: { requiresAuth: true }
            },
            {
                path: 'forms/:id',
                name: 'FormDetail',
                component: FormDetail,
                meta: { requiresAuth: true }
            },
            {
                path: 'blog',
                name: 'BlogList',
                component: BlogList,
                meta: { requiresAuth: true }
            },
            {
                path: 'blog/create',
                name: 'BlogCreate',
                component: BlogEditor,
                meta: { requiresAuth: true }
            },
            {
                path: 'blog/edit/:id',
                name: 'BlogEdit',
                component: BlogEditor,
                meta: { requiresAuth: true }
            },
            {
                path: 'news',
                name: 'NewsList',
                component: NewsList,
                meta: { requiresAuth: true }
            },
            {
                path: 'news/create',
                name: 'NewsCreate',
                component: NewsEditor,
                meta: { requiresAuth: true }
            },
            {
                path: 'news/edit/:id',
                name: 'NewsEdit',
                component: NewsEditor,
                meta: { requiresAuth: true }
            },
        ]
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

router.beforeEach((to, from, next) => {
    const adminUser = localStorage.getItem('adminUser');

    // 如果要去 dashboard 相關頁面但沒有登入，導向登入頁
    if (to.path.startsWith('/dashboard') && !adminUser) {
        next('/');
    }
    // 如果已經登入但要去登入頁，導向 dashboard
    else if (to.path === '/login' && adminUser) {
        next('/dashboard');
    }
    else {
        next();
    }
});

export default router;