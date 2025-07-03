<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-r from-blue-50 to-blue-100 px-4">
    <form
      @submit.prevent="onSubmit"
      class="bg-white p-8 rounded-2xl shadow-lg w-full max-w-md border border-gray-200"
      novalidate
    >
      <h2 class="text-3xl font-bold mb-6 text-center text-blue-700">Register</h2>

      <!-- Username -->
      <div class="mb-5">
        <label for="username" class="block text-gray-700 mb-1 font-medium">Username</label>
        <input
          id="username"
          v-model="username"
          type="text"
          placeholder="Choose a username"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 transition"
          required
          minlength="4"
          pattern="^\S+$"
        />
      </div>

      <!-- Password -->
      <div class="mb-5 relative">
        <label for="password" class="block text-gray-700 mb-1 font-medium">Password</label>
        <input
          :type="showPassword ? 'text' : 'password'"
          id="password"
          v-model="password"
          placeholder="Create a password"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 transition pr-10"
          required
          minlength="8"
        />
        <button
          type="button"
          @click="toggleShowPassword"
          class="absolute right-3 top-9 text-gray-500 hover:text-blue-600 focus:outline-none"
          tabindex="-1"
        >
          <span v-if="showPassword">🙈</span>
          <span v-else>👁️</span>
        </button>
      </div>

      <!-- Confirm Password -->
      <div class="mb-6 relative">
        <label for="confirmPassword" class="block text-gray-700 mb-1 font-medium">Confirm Password</label>
        <input
          :type="showConfirmPassword ? 'text' : 'password'"
          id="confirmPassword"
          v-model="confirmPassword"
          placeholder="Confirm your password"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 transition pr-10"
          required
          minlength="8"
        />
        <button
          type="button"
          @click="toggleShowConfirmPassword"
          class="absolute right-3 top-9 text-gray-500 hover:text-blue-600 focus:outline-none"
          tabindex="-1"
        >
          <span v-if="showConfirmPassword">🙈</span>
          <span v-else>👁️</span>
        </button>
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors duration-200"
      >
        {{ loading ? "Registering..." : "Register" }}
      </button>

      <p class="mt-6 text-center text-sm text-gray-600">
        Already have an account?
        <router-link to="/login" class="text-blue-600 hover:underline font-medium">
          Login here
        </router-link>
      </p>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import axios from 'axios';
import alertify from 'alertifyjs';
import 'alertifyjs/build/css/alertify.min.css';

alertify.set('notifier', 'position', 'top-right');

const username = ref('');
const password = ref('');
const confirmPassword = ref('');
const loading = ref(false);
const showPassword = ref(false);
const showConfirmPassword = ref(false);

function toggleShowPassword() {
  showPassword.value = !showPassword.value;
}
function toggleShowConfirmPassword() {
  showConfirmPassword.value = !showConfirmPassword.value;
}

function isStrongPassword(pwd: string): boolean {
  const pattern = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$/;
  return pattern.test(pwd);
}

function validateUsername(uname: string): boolean {
  if (uname.length < 4) return false;
  if (/\s/.test(uname)) return false;
  return true;
}

async function onSubmit() {
  if (loading.value) return;

  // Validate
  if (!validateUsername(username.value)) {
    alertify.error('Username must be at least 4 characters and contain no spaces.');
    return;
  }

  if (!isStrongPassword(password.value)) {
    alertify.error('Password must be at least 8 characters, include uppercase, lowercase, number, and special character.');
    return;
  }

  if (password.value !== confirmPassword.value) {
    alertify.error('Passwords do not match.');
    return;
  }

  loading.value = true;

  try {
    const response = await axios.post('http://localhost:5246/api/Auth/register', {
      username: username.value,
      password: password.value
    });

    // Handle success
    alertify.success(response.data.message || `Registered as ${username.value}`);
    username.value = '';
    password.value = '';
    confirmPassword.value = '';
  } catch (error: any) {
    if (error.response?.data?.errors) {
      error.response.data.errors.forEach((err: string) => alertify.error(err));
    } else if (error.response?.data?.message) {
      alertify.error(error.response.data.message);
    } else {
      alertify.error('Registration failed. Please try again.');
    }
  } finally {
    loading.value = false;
  }
}
</script>
