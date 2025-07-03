import { createRouter, createWebHistory } from 'vue-router';
import LoginForm from '../components/Auth/LoginForm.vue';
import RegisterView from '../components/Auth/RegisterView.vue';
import NoteIndex from '../views/note/Index.vue';
import SortIcon from '../views/note/SortIcon.vue'; // Ensure the path is correct


const routes = [
  { path: '/login', component: LoginForm },
  { path: '/register', component: RegisterView },
  { path: '/', redirect: '/login' },
  { path: '/note', component: NoteIndex },
  { path: '/note', component: NoteIndex },
  { path: '/note/sort', component: SortIcon },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
