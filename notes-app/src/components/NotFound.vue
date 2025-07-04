<template>
  <div class="not-found">
    <h1>404 - Page Not Found</h1>
    <p>Sorry, the page you are looking for does not exist.</p>
    <router-link to="/">Go back home</router-link>
  </div>
</template>

<style scoped>
.not-found {
  text-align: center;
  margin-top: 100px;
  font-family: Arial, sans-serif;
}

h1 {
  font-size: 3em;
  color: #ff4c4c;
}

p {
  margin: 20px 0;
  font-size: 1.2em;
}

a {
  color: #007bff;
  text-decoration: underline;
  cursor: pointer;
}
</style>
