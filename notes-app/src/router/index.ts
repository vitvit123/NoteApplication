import { createRouter, createWebHistory } from 'vue-router';
import LoginForm from '../components/Auth/LoginForm.vue';
import RegisterView from '../components/Auth/RegisterView.vue';
import NoteIndex from '../views/note/Index.vue';
import SortIcon from '../views/note/SortIcon.vue';
import NotFound from '../components/NotFound.vue';

const routes = [
  { path: '/login', component: LoginForm },
  { path: '/register', component: RegisterView },
  { path: '/', redirect: '/login' },
  { path: '/note', component: NoteIndex },
  { path: '/note/sort', component: SortIcon },
  { path: '/:pathMatch(.*)*', name: 'NotFound', component: NotFound },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');

  if ((to.path === '/login' || to.path === '/register') && token) {
    next('/note');
  } else {
    next();
  }
});

export default router;
