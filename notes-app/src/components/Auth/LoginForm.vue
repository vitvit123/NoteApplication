<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gradient-to-r from-blue-50 to-blue-100 px-4"
  >
    <form
      @submit.prevent="onSubmit"
      class="bg-white p-8 rounded-2xl shadow-lg w-full max-w-md border border-gray-200"
    >
      <h2 class="text-3xl font-bold mb-6 text-center text-blue-700">
        Note Application
      </h2>

      <div class="mb-5">
        <label for="username" class="block text-gray-700 mb-1 font-medium">
          Username
        </label>
        <input
          id="username"
          v-model="username"
          type="text"
          placeholder="Enter your username"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition"
          required
        />
      </div>

      <div class="mb-6 relative">
        <label for="password" class="block text-gray-700 mb-1 font-medium">
          Password
        </label>
        <input
          :type="showPassword ? 'text' : 'password'"
          id="password"
          v-model="password"
          placeholder="Enter your password"
          class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400 focus:border-blue-400 transition pr-10"
          required
        />
        <button
          type="button"
          @click="toggleShowPassword"
          class="absolute right-3 top-9 text-gray-500 hover:text-blue-600 focus:outline-none"
          tabindex="-1"
          aria-label="Toggle password visibility"
        >
          <svg
            v-if="showPassword"
            xmlns="http://www.w3.org/2000/svg"
            class="h-6 w-6"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M13.875 18.825A10.05 10.05 0 0112 19c-5.523 0-10-4.477-10-10a9.96 9.96 0 014.75-8.5m1.875 3.75a3 3 0 114.5 4.5"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
            />
          </svg>

          <svg
            v-else
            xmlns="http://www.w3.org/2000/svg"
            class="h-6 w-6"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M13.875 18.825A10.05 10.05 0 0112 19c-5.523 0-10-4.477-10-10a9.96 9.96 0 014.75-8.5m1.875 3.75a3 3 0 114.5 4.5"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M3 3l18 18"
            />
          </svg>
        </button>
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors duration-200 flex justify-center items-center gap-2"
      >
        <svg
          v-if="loading"
          class="animate-spin h-5 w-5 text-white"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"
          ></path>
        </svg>
        {{ loading ? "Logging in..." : "Login" }}
      </button>

      <p class="mt-6 text-center text-sm text-gray-600">
        Don't have an account?
        <router-link
          to="/register"
          class="text-blue-600 hover:underline font-medium"
        >
          Register here
        </router-link>
      </p>
    </form>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import axios from "axios";
import alertify from "alertifyjs";
import "alertifyjs/build/css/alertify.css";
import router from "../../router";

// Position alertify notifications top-right
alertify.set("notifier", "position", "top-right");

const username = ref("");
const password = ref("");
const loading = ref(false);
const errorMessage = ref("");
const showPassword = ref(false);

function toggleShowPassword() {
  showPassword.value = !showPassword.value;
}

async function onSubmit() {
    console.log("onSubmit called");
  if (loading.value) return; 

  errorMessage.value = "";
  loading.value = true;

  try {
    const response = await axios.post("http://localhost:5246/api/Auth/login", {
      username: username.value,
      password: password.value,
    });

    if (response.status === 200 && response.data.token) {
        alertify.success(`Logged in as ${response.data.username}`);
        alertify.success('token', response.data.token);

        localStorage.setItem('token', response.data.token);
        localStorage.setItem('username', response.data.username);
        localStorage.setItem('userId', response.data.userId);

      router.push('/note');
    } else {
      errorMessage.value =
        response.data.message || "Login failed. Please try again.";
      alertify.error(errorMessage.value);
    }
  } catch (error: any) {
    if (error.response) {
      if (typeof error.response.data === "string") {
        errorMessage.value = error.response.data;
      } else if (error.response.data.message) {
        errorMessage.value = error.response.data.message;
      } else {
        errorMessage.value = "Login failed. Please try again.";
      }
    } else {
      errorMessage.value = "Login failed. Please try again.";
    }
    alertify.error(errorMessage.value);
  } finally {
    loading.value = false;
  }
}
</script>
