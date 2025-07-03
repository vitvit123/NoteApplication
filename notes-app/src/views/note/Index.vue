<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Task Bar -->
    <nav class="bg-blue-600 text-white p-4 flex justify-between items-center">
      <div class="text-lg font-bold">Note Application</div>
      <div>
        <span class="mr-4">Hello, {{ username }}</span>
        <button
          @click="logout"
          class="bg-red-500 px-3 py-1 rounded hover:bg-red-600"
        >
          Logout
        </button>
      </div>
    </nav>

    <main class="p-6 max-w-5xl mx-auto">
      <h1 class="text-2xl font-semibold mb-4">Your Notes</h1>

      <!-- Search Input -->
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Search notes..."
        class="mb-4 p-2 border rounded w-full max-w-sm"
      />

      <!-- Add Note Button -->
      <button
        @click="openAddModal"
        class="mb-4 bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
      >
        + Add Note
      </button>

      <!-- Notes Table -->
      <table class="min-w-full bg-white shadow rounded overflow-hidden">
        <thead class="bg-gray-200">
          <tr>
            <th
              class="text-left px-4 py-2 cursor-pointer"
              @click="sortBy('title')"
            >
              Title
              <SortIcon
                :field="'title'"
                :sortField="sortField"
                :sortAsc="sortAsc"
              />
            </th>
            <th
              class="text-left px-4 py-2 cursor-pointer"
              @click="sortBy('content')"
            >
              Content
              <SortIcon
                :field="'content'"
                :sortField="sortField"
                :sortAsc="sortAsc"
              />
            </th>
            <th
              class="text-left px-4 py-2 cursor-pointer"
              @click="sortBy('createdAt')"
            >
              Created At
              <SortIcon
                :field="'createdAt'"
                :sortField="sortField"
                :sortAsc="sortAsc"
              />
            </th>
            <th
              class="text-left px-4 py-2 cursor-pointer"
              @click="sortBy('updatedAt')"
            >
              Updated At
              <SortIcon
                :field="'updatedAt'"
                :sortField="sortField"
                :sortAsc="sortAsc"
              />
            </th>
            <th class="px-4 py-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="note in pagedNotes"
            :key="note.id"
            class="border-b hover:bg-gray-50"
          >
            <td class="px-4 py-2">{{ note.title }}</td>
            <td class="px-4 py-2">{{ note.content }}</td>
            <td class="px-4 py-2">{{ formatDate(note.createdAt) }}</td>
            <td class="px-4 py-2">{{ formatDate(note.updatedAt) }}</td>
            <td class="px-4 py-2">
              <button
                @click="openEditModal(note)"
                class="text-blue-600 hover:underline mr-2"
              >
                Edit
              </button>
              <button
                @click="deleteNote(note.id)"
                class="text-red-600 hover:underline"
              >
                Delete
              </button>
            </td>
          </tr>
          <tr v-if="filteredNotes.length === 0">
            <td colspan="5" class="text-center py-4 text-gray-500">
              No notes found.
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination Controls -->
      <div class="mt-4 flex justify-center items-center space-x-4">
        <button
          :disabled="currentPage === 1"
          @click="prevPage"
          class="px-3 py-1 rounded bg-gray-300 hover:bg-gray-400 disabled:opacity-50"
        >
          Prev
        </button>
        <span>Page {{ currentPage }} of {{ totalPages }}</span>
        <button
          :disabled="currentPage === totalPages"
          @click="nextPage"
          class="px-3 py-1 rounded bg-gray-300 hover:bg-gray-400 disabled:opacity-50"
        >
          Next
        </button>
      </div>
    </main>

    <!-- Add/Edit Note Modal -->
    <div
      v-if="modalVisible"
      class="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50"
    >
      <div class="bg-white p-6 rounded shadow-lg w-full max-w-md">
        <h2 class="text-xl mb-4">
          {{ isEditMode ? "Edit Note" : "Add Note" }}
        </h2>
        <form @submit.prevent="submitNote">
          <label class="block mb-2">
            Title:
            <input
              v-model="modalNote.title"
              required
              class="w-full border p-2 rounded"
            />
          </label>
          <label class="block mb-4">
            Content:
            <textarea
              v-model="modalNote.content"
              class="w-full border p-2 rounded"
              rows="4"
            ></textarea>
          </label>
          <div class="flex justify-end space-x-2">
            <button
              type="button"
              @click="closeModal"
              class="px-4 py-2 rounded bg-gray-300 hover:bg-gray-400"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700"
            >
              {{ isEditMode ? "Update" : "Add" }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import axios from "axios";
import alertify from "alertifyjs";
import SortIcon from "./SortIcon.vue";

// Configure alertify once
alertify.set('notifier','position', 'top-right');

// User info
const username = ref("");
const userId = ref("");

// Notes and UI state
const notes = ref<any[]>([]);
const searchQuery = ref("");
const currentPage = ref(1);
const pageSize = 5;

const modalVisible = ref(false);
const isEditMode = ref(false);
const modalNote = ref({ id: 0, title: "", content: "" });

// Load user info and notes on mount
onMounted(async () => {
  username.value = localStorage.getItem("username") || "";
  userId.value = localStorage.getItem("userId") || "";

  if (!userId.value || !localStorage.getItem("token")) {
    alertify.error("Session expired. Please log in.");
    window.location.href = "/login";
    return;
  }

  await loadNotes();
});

async function loadNotes() {
  try {
    const token = localStorage.getItem("token");

    if (!token) {
      alertify.error("Please log in first.");
      logout();
      return;
    }

    const response = await axios.get("http://localhost:5246/api/notes", {
      headers: {
        Authorization: `Bearer ${token}`
      },
    });

    notes.value = response.data;
  } catch (error) {
    console.error("Error loading notes:", error);
    alertify.error("Failed to load notes. Please check your API server or log in again.");
    logout();
  }
}

// Filtered notes based on search
const filteredNotes = computed(() => {
  if (!searchQuery.value) return notes.value;
  const q = searchQuery.value.toLowerCase();
  return notes.value.filter(
    (note) =>
      note.title.toLowerCase().includes(q) ||
      (note.content && note.content.toLowerCase().includes(q))
  );
});

// Pagination
const totalPages = computed(() =>
  Math.max(1, Math.ceil(filteredNotes.value.length / pageSize))
);

const pagedNotes = computed(() => {
  const start = (currentPage.value - 1) * pageSize;
  return filteredNotes.value.slice(start, start + pageSize);
});

function prevPage() {
  if (currentPage.value > 1) currentPage.value--;
}
function nextPage() {
  if (currentPage.value < totalPages.value) currentPage.value++;
}

// Date formatting
function formatDate(dateString: string) {
  const d = new Date(dateString);
  return d.toLocaleString();
}

// Logout
function logout() {
  localStorage.clear();
  alertify.warning("You have been logged out.");
  window.location.href = "/login";
}

// Modal controls
function openAddModal() {
  isEditMode.value = false;
  modalNote.value = { id: 0, title: "", content: "" };
  modalVisible.value = true;
}
function openEditModal(note: any) {
  isEditMode.value = true;
  modalNote.value = { ...note };
  modalVisible.value = true;
}
function closeModal() {
  modalVisible.value = false;
}

// Add or update note
async function submitNote() {
  const token = localStorage.getItem("token");
  try {
    if (!modalNote.value.title.trim()) {
      alertify.error("Title is required.");
      return;
    }

    if (isEditMode.value) {
      // Update note
      await axios.put(
        `http://localhost:5246/api/notes/${modalNote.value.id}`,
        modalNote.value,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      alertify.success("Note updated successfully.");
    } else {
      console.log("Adding note payload:", modalNote.value);
      const response = await axios.post(
        "http://localhost:5246/api/notes",
        modalNote.value,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      console.log("Add note response:", response);
      alertify.success("Note added successfully.");
    }
    await loadNotes();
    closeModal();
  } catch (error) {
    console.error("Add note error:", error.response || error);
    alertify.error("Failed to save note.");
  }
}


// Delete note
async function deleteNote(id: number) {
  const token = localStorage.getItem("token");

  alertify.confirm(
    "Confirm Delete",
    "Are you sure you want to delete this note?",
    async function () {
      try {
        await axios.delete(`http://localhost:5246/api/notes/${id}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        alertify.success("Note deleted successfully.");
        await loadNotes();
      } catch (error) {
        console.error(error);
        alertify.error("Failed to delete note.");
      }
    },
    function () {
      // Cancel callback, do nothing
    }
  ).set("labels", { ok: "Yes", cancel: "No" });
}


// Sort logic
const sortField = ref("createdAt");
const sortAsc = ref(true);

function sortBy(field: string) {
  if (sortField.value === field) {
    sortAsc.value = !sortAsc.value;
  } else {
    sortField.value = field;
    sortAsc.value = true;
  }

  notes.value.sort((a: any, b: any) => {
    let valA = a[field];
    let valB = b[field];

    if (typeof valA === "string") valA = valA.toLowerCase();
    if (typeof valB === "string") valB = valB.toLowerCase();

    if (valA < valB) return sortAsc.value ? -1 : 1;
    if (valA > valB) return sortAsc.value ? 1 : -1;
    return 0;
  });
}
</script>
