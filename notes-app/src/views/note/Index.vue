<template>
  <div class="min-h-screen bg-gray-900 text-gray-200 flex flex-col">
    <!-- Navbar -->
    <nav class="bg-gray-800 text-gray-200 shadow-lg p-4 flex justify-between items-center select-none">
      <h1 class="text-xl font-bold select-none">📝 Note Application</h1>
      <div class="flex items-center space-x-4">
        <span class="font-medium">Hello, <span class="underline text-blue-400">{{ username }}</span></span>
        <button
          @click="logout"
          class="bg-red-700 hover:bg-red-800 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-1 transition-colors duration-200 rounded px-3 py-1 text-sm font-semibold shadow"
          title="Logout"
        >
          Logout
        </button>
      </div>
    </nav>

    <!-- Main Content -->
    <main class="flex-grow max-w-5xl mx-auto p-6 w-full px-4">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-6 gap-4">
        <h2 class="text-3xl font-extrabold text-gray-100 select-none">Your Notes</h2>

        <button
          @click="openAddModal"
          class="bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-400 focus:ring-offset-1 transition-colors duration-200 text-white font-semibold px-5 py-2 rounded shadow-sm shadow-green-500/50"
        >
          + Add Note
        </button>
      </div>

      <!-- Search Input -->
      <input
        v-model="searchQuery"
        type="text"
        placeholder="Search notes..."
        class="w-full max-w-sm p-3 rounded-md border border-gray-700 bg-gray-800 placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent mb-6 transition text-gray-200"
        aria-label="Search notes"
      />

      <!-- Notes Table -->
      <div class="sm:overflow-x-visible overflow-x-auto rounded-lg shadow-lg border border-gray-700 bg-gray-800">
        <table class="w-full text-left border-collapse min-w-[500px]">
          <thead class="bg-gray-700">
            <tr>
              <th class="px-5 py-3 text-blue-400 font-semibold uppercase tracking-wide select-none">No</th>

              <th
                class="cursor-pointer px-5 py-3 text-blue-400 font-semibold uppercase tracking-wide select-none"
                @click="sortBy('createdAt')"
              >
                Created At
                <SortIcon :field="'createdAt'" :sortField="sortField" :sortAsc="sortAsc" />
              </th>

              <th class="px-5 py-3 text-blue-400 font-semibold uppercase tracking-wide select-none text-center">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(note, index) in pagedNotes"
              :key="note.id"
              class="border-t border-gray-700 even:bg-gray-800 hover:bg-gray-700 transition"
            >
              <td class="px-5 py-3 whitespace-nowrap font-medium text-gray-200 text-center">
                {{ (currentPage - 1) * pageSize + index + 1 }}
              </td>
              <td class="px-5 py-3 whitespace-nowrap text-gray-400 font-mono text-sm">
                {{ formatDate(note.createdAt) }}
              </td>
              <td class="px-5 py-3 whitespace-nowrap text-center relative">
                <div class="inline-block text-left">
                  <button
                    @click="toggleActions(note.id)"
                    class="px-3 py-1 rounded border border-gray-600 bg-gray-900 hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-1 transition-colors duration-150 flex items-center gap-1"
                    aria-haspopup="true"
                    :aria-expanded="isActionsOpen(note.id) ? 'true' : 'false'"
                    title="Actions"
                  >
                    <svg
                      class="w-5 h-5 text-blue-400"
                      fill="none"
                      stroke="currentColor"
                      stroke-width="2"
                      viewBox="0 0 24 24"
                    >
                      <circle cx="5" cy="12" r="2" />
                      <circle cx="12" cy="12" r="2" />
                      <circle cx="19" cy="12" r="2" />
                    </svg>
                    Actions
                  </button>
                  <transition name="fade">
                    <div
                      v-if="isActionsOpen(note.id)"
                      @click.away="closeActions"
                      class="absolute right-0 mt-2 w-40 bg-gray-900 border border-gray-700 rounded shadow-lg z-10 ring-1 ring-black ring-opacity-50 transition ease-in-out duration-150"
                    >
                      <button
                        @click.stop="openDetailModal(note); closeActions()"
                        class="block w-full text-left px-4 py-2 hover:bg-blue-700 text-gray-200"
                      >
                        View Detail
                      </button>
                      <button
                        @click.stop="openEditModal(note); closeActions()"
                        class="block w-full text-left px-4 py-2 hover:bg-indigo-700 text-gray-200"
                      >
                        Edit
                      </button>
                      <button
                        @click.stop="deleteNote(note.id); closeActions()"
                        class="block w-full text-left px-4 py-2 text-red-500 hover:bg-red-700"
                      >
                        Delete
                      </button>
                    </div>
                  </transition>
                </div>
              </td>
            </tr>

            <tr v-if="filteredNotes.length === 0" class="text-center">
              <td colspan="5" class="py-10 text-gray-500 italic select-none">
                No notes found.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination Controls -->
      <div class="mt-6 flex justify-center items-center space-x-6 select-none text-gray-400 font-semibold">
        <button
          :disabled="currentPage === 1"
          @click="prevPage"
          class="px-4 py-2 rounded bg-gray-700 hover:bg-gray-600 disabled:opacity-40 transition"
        >
          Prev
        </button>
        <span>
          Page <strong>{{ currentPage }}</strong> of <strong>{{ totalPages }}</strong>
        </span>
        <button
          :disabled="currentPage === totalPages"
          @click="nextPage"
          class="px-4 py-2 rounded bg-gray-700 hover:bg-gray-600 disabled:opacity-40 transition"
        >
          Next
        </button>
      </div>
    </main>

    <!-- Add/Edit Note Modal -->
    <div
      v-if="modalVisible"
      class="fixed inset-0 bg-black bg-opacity-70 flex items-center justify-center z-50"
      aria-modal="true"
      role="dialog"
      aria-labelledby="modal-title"
    >
      <div class="bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6 text-gray-200" @click.stop>
        <h3 id="modal-title" class="text-2xl font-bold mb-5 select-none">
          {{ isEditMode ? "Edit Note" : "Add Note" }}
        </h3>
        <form @submit.prevent="submitNote" class="space-y-5">
          <div>
            <label for="note-title" class="block font-semibold mb-1">
              Title <span class="text-red-500">*</span>
            </label>
            <input
              id="note-title"
              v-model="modalNote.title"
              type="text"
              required
              class="w-full rounded border border-gray-600 bg-gray-900 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 transition text-gray-200"
            />
          </div>

          <div>
            <label for="note-content" class="block font-semibold mb-1">
              Content
            </label>
            <textarea
              id="note-content"
              v-model="modalNote.content"
              rows="5"
              class="w-full rounded border border-gray-600 bg-gray-900 px-3 py-2 resize-none focus:outline-none focus:ring-2 focus:ring-blue-500 transition text-gray-200"
            ></textarea>
          </div>

          <div class="flex justify-end gap-3">
            <button
              type="button"
              @click="closeModal"
              class="px-4 py-2 rounded border border-gray-600 hover:bg-gray-700 transition text-gray-200"
            >
              Cancel
            </button>
            <button
              type="submit"
              class="px-4 py-2 rounded bg-blue-600 hover:bg-blue-700 transition text-white font-semibold"
            >
              {{ isEditMode ? "Update" : "Add" }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Note Detail Modal -->
    <div
      v-if="detailVisible"
      class="fixed inset-0 bg-black bg-opacity-70 flex items-center justify-center z-50"
      aria-modal="true"
      role="dialog"
      aria-labelledby="detail-modal-title"
    >
      <div class="bg-gray-800 rounded-lg shadow-xl max-w-md w-full p-6 text-gray-200" @click.stop>
        <h3 id="detail-modal-title" class="text-2xl font-bold mb-4 select-none">
          Note Details
        </h3>
        <div class="mb-2">
          <strong>Title:</strong>
          <p class="ml-2 inline">{{ detailNote.title }}</p>
        </div>
        <div class="mb-4">
          <strong>Content:</strong>
          <p class="ml-2 whitespace-pre-line max-h-48 overflow-y-auto">{{ detailNote.content }}</p>
        </div>
        <div class="text-sm text-gray-400 mb-2 font-mono">
          <div>Created: {{ formatDate(detailNote.createdAt) }}</div>
          <div>Updated: {{ formatDate(detailNote.updatedAt) }}</div>
        </div>
        <div class="flex justify-end">
          <button
            @click="closeDetailModal"
            class="px-4 py-2 rounded border border-gray-600 hover:bg-gray-700 transition text-gray-200"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  </div>
</template>


<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import axios from "axios";
import alertify from "alertifyjs";
import SortIcon from "./SortIcon.vue";

// Detail modal state
const detailVisible = ref(false);
const detailNote = ref<any>({});

// Open/close detail modal
function openDetailModal(note: any) {
  detailNote.value = note;
  detailVisible.value = true;
  closeActions();
}
function closeDetailModal() {
  detailVisible.value = false;
}

// Alertify config
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
    const userId = localStorage.getItem("userId"); 

    if (!token || !userId) {
      alertify.error("Please log in first.");
      logout();
      return;
    }

    const response = await axios.get("http://localhost:5246/api/notes", {
      headers: {
        Authorization: `Bearer ${token}`,
        "x-user-id": userId
      },
    });

    notes.value = response.data;
  } catch (error) {
    console.error("Error loading notes:", error);
    alertify.error("Failed to load notes.");
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
  const date = d.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
  const time = d.toLocaleTimeString('en-US', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true
  });
  return `${date} at ${time}`;
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
  closeActions();
}
function closeModal() {
  modalVisible.value = false;
}

// Add or update note
async function submitNote() {
  const token = localStorage.getItem("token");
  const userId = localStorage.getItem("userId"); 
  if (!token || !userId) {
    alertify.error("Please log in first.");
    logout();
    return;
  }
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
        { headers: {
          Authorization: `Bearer ${token}`,
          "x-user-id": userId
        }, }
      );
      alertify.success("Note updated successfully.");
    } else {
      await axios.post(
        "http://localhost:5246/api/notes",
        modalNote.value,
        { headers: {
          Authorization: `Bearer ${token}`,
          "x-user-id": userId
        },}
      );
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
  const userId = localStorage.getItem("userId"); 

  if (!token || !userId) {
    alertify.error("Please log in first.");
    logout();
    return;
  }
  alertify.confirm(
    "Confirm Delete",
    "Are you sure you want to delete this note?",
    async function () {
      try {
        await axios.delete(`http://localhost:5246/api/notes/${id}`, {
          headers: {
            Authorization: `Bearer ${token}`,
            "x-user-id": userId
          },
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

// Dropdown Actions menu logic
const openActionsId = ref<number | null>(null);

function toggleActions(id: number) {
  openActionsId.value = openActionsId.value === id ? null : id;
}

function closeActions() {
  openActionsId.value = null;
}

function isActionsOpen(id: number) {
  return openActionsId.value === id;
}
</script>

