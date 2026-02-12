document.addEventListener("DOMContentLoaded", function () {

    // 🔐 EXIT if not on Books page
    const bookPage = document.getElementById("books-page");
    if (!bookPage) return;

    /* ================= COMMON ================= */
    const actionMessage = document.getElementById("actionMessage");

    function showMessage(message, success) {
        actionMessage.textContent = message;
        actionMessage.classList.remove("d-none", "alert-success", "alert-danger");
        actionMessage.classList.add(success ? "alert-success" : "alert-danger");
    }

    function closeModal(modalEl) {
        const instance = bootstrap.Modal.getInstance(modalEl);
        if (instance) instance.hide();
        document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        document.body.classList.remove('modal-open');

        // Make sure no buttons are stuck disabled
        modalEl.querySelectorAll('button[type="submit"]').forEach(btn => btn.disabled = false);
    }


    /* ================= MODAL ELEMENTS ================= */
    const updateModalEl = document.getElementById("updateModal");
    const archiveModalEl = document.getElementById("archiveModal");

    const updateForm = document.getElementById("updateForm");
    const archiveForm = document.getElementById("archiveForm");

    /* ================= RESET UPDATE BUTTON ON MODAL OPEN ================= */
    updateModalEl?.addEventListener('show.bs.modal', function () {
        const submitBtn = updateModalEl.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = false;       // ensure enabled
            submitBtn.classList.remove("disabled"); // remove Bootstrap grey styling
        }
    });

    const updateTitleInput = document.getElementById("updateTitle");
    const updateBookIdInput = document.getElementById("updateBookId");
    const updatePublishDateInput = document.getElementById("updatePublishDate");


    /* ================= ARCHIVE MODAL FIX ================= */
    archiveModalEl?.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const bookId = button.getAttribute('data-book-id');
        document.getElementById("archiveBookId").value = bookId;
    });

    /* ================= EVENT DELEGATION ================= */
    document.addEventListener("click", function (e) {

        // -------- UPDATE BUTTON ----------
        const updateBtn = e.target.closest(".update-btn");
        if (updateBtn) {
            e.preventDefault();
            const row = updateBtn.closest("tr");
            const id = updateBtn.dataset.bookId;

            // Fill book ID and title
            updateBookIdInput.value = id;
            updateTitleInput.value = row.querySelector(".title").textContent.trim();

            // Prefill category select
            const categorySelect = document.getElementById("updateCategorySelect");
            const categoryId = row.dataset.categoryId; // Make sure your <tr> has data-category-id
            if (categoryId) categorySelect.value = categoryId;

            // Fill publish date
            const publishDateText = row.querySelector("td[data-publish-date]")?.dataset.publishDate;
            if (publishDateText) {
                updatePublishDateInput.value = publishDateText;
            }

            // Show modal
            const modalInstance = new bootstrap.Modal(updateModalEl);
            modalInstance.show();

            // Scroll selected category into view after modal is fully shown
            updateModalEl.addEventListener('shown.bs.modal', function handler() {
                const container = document.getElementById("updateCategoriesContainer");
                const selectedRadio = container.querySelector(".update-category-radio:checked");
                if (selectedRadio) {
                    selectedRadio.scrollIntoView({ block: "center", behavior: "smooth" });
                }
                updateModalEl.removeEventListener('shown.bs.modal', handler);
            });

            return;
        }

        // -------- PAGINATION ----------
        const pageLink = e.target.closest("a.page-link");
        if (pageLink && pageLink.dataset.page) {
            e.preventDefault();
            const page = parseInt(pageLink.dataset.page);
            const pageSize = parseInt(document.getElementById("pageSizeSelect")?.value || 10);
            loadBooks(page, pageSize);
            return;
        }
    });

    /* ================= UPDATE FORM SUBMIT ================= */
    updateForm?.addEventListener("submit", function (e) {
        e.preventDefault();

        const submitBtn = updateForm.querySelector('button[type="submit"]');
        submitBtn.disabled = true; // disable button while submitting
        submitBtn.classList.add("disabled"); // optional, remove grey effect if needed

        const id = parseInt(updateBookIdInput.value);
        const title = updateTitleInput.value.trim();

        if (!title) {
            showMessage("Title is required", false);
            submitBtn.disabled = false; // re-enable immediately
            submitBtn.classList.remove("disabled");
            return;
        }

        // Selected category from dropdown
        const selectedCategoryId = document.getElementById("updateCategorySelect").value;
        if (!selectedCategoryId) {
            showMessage("Please select a category.", false);
            submitBtn.disabled = false;
            submitBtn.classList.remove("disabled");
            return;
        }

        fetch(`/Book/UpdateBook/${id}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                Id: id,
                Title: title,
                CategoryId: parseInt(selectedCategoryId) // ensure number
            })
        })
            .then(r => r.json())
            .then(data => {
                submitBtn.disabled = false; // re-enable button
                submitBtn.classList.remove("disabled");

                if (data.success) {
                    loadBooks(getCurrentPage(), getCurrentPageSize());
                    closeModal(updateModalEl);
                    showMessage(data.message, true);
                } else {
                    showMessage(data.message, false);
                }
            })
            .catch(err => {
                submitBtn.disabled = false; // re-enable on error
                submitBtn.classList.remove("disabled");
                showMessage("Unexpected error: " + err, false);
            });
    });


    /* ================= ARCHIVE FORM SUBMIT ================= */
    archiveForm?.addEventListener("submit", function (e) {
        e.preventDefault();

        const id = parseInt(document.getElementById("archiveBookId").value);

        fetch(`/Book/ArchiveBook/${id}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(id)
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    loadBooks(getCurrentPage(), getCurrentPageSize());
                    closeModal(archiveModalEl);
                    showMessage(data.message, true);
                } else {
                    showMessage(data.message, false);
                }
            })
            .catch(err => showMessage("Unexpected error: " + err, false));
    });

    /* ================= PAGE SIZE CHANGE ================= */
    const pageSizeSelect = document.getElementById("pageSizeSelect");
    pageSizeSelect?.addEventListener("change", function () {
        loadBooks(1, parseInt(this.value));
    });

    /* ================= AJAX LOAD FUNCTION ================= */
    function loadBooks(page, pageSize) {
        const search = document.querySelector('input[name="search"]')?.value || '';
        const filter = document.querySelector('input[name="filter"]')?.value || '';

        const url = `/Book/Index?page=${page}&pageSize=${pageSize}&search=${encodeURIComponent(search)}&filter=${encodeURIComponent(filter)}&ajax=true`;

        fetch(url)
            .then(r => r.text())
            .then(html => {
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, "text/html");

                const newTable = doc.getElementById("booksTableContainer");
                const oldTable = document.getElementById("booksTableContainer");
                if (newTable && oldTable) oldTable.replaceWith(newTable);

                const newPagination = doc.querySelector("nav[aria-label='Books pagination']");
                const oldPagination = document.querySelector("nav[aria-label='Books pagination']");
                if (newPagination && oldPagination) oldPagination.replaceWith(newPagination);

                let currentPageInput = document.getElementById("currentPage");
                if (!currentPageInput) {
                    currentPageInput = document.createElement("input");
                    currentPageInput.type = "hidden";
                    currentPageInput.id = "currentPage";
                    document.getElementById("books-page").appendChild(currentPageInput);
                }
                currentPageInput.value = page;
            })
            .catch(err => showMessage("Failed to load page: " + err, false));
    }

    function getCurrentPage() {
        return parseInt(document.getElementById("currentPage")?.value || 1);
    }

    function getCurrentPageSize() {
        return parseInt(document.getElementById("pageSizeSelect")?.value || 10);
    }

});
