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
    }

    /* ================= MODAL ELEMENTS ================= */
    const updateModalEl = document.getElementById("updateModal");
    const archiveModalEl = document.getElementById("archiveModal");

    const updateForm = document.getElementById("updateForm");
    const archiveForm = document.getElementById("archiveForm");

    const updateTitleInput = document.getElementById("updateTitle");
    const updateBookIdInput = document.getElementById("updateBookId");

    /* ================= EVENT DELEGATION ================= */
    document.addEventListener("click", function (e) {

        // -------- UPDATE BUTTON ----------
        const updateBtn = e.target.closest(".update-btn");
        if (updateBtn) {
            e.preventDefault();
            const row = updateBtn.closest("tr");
            const id = updateBtn.dataset.bookId;
            const title = row.querySelector(".title").textContent;

            updateTitleInput.value = title;
            updateBookIdInput.value = id;

            new bootstrap.Modal(updateModalEl).show();
            return;
        }

        // -------- ARCHIVE BUTTON ----------
        const archiveBtn = e.target.closest(".archive-btn");
        if (archiveBtn) {
            e.preventDefault();
            document.getElementById("archiveBookId").value = archiveBtn.dataset.bookId;
            new bootstrap.Modal(archiveModalEl).show();
            return;
        }

        // -------- PAGINATION ----------
        const pageLink = e.target.closest("a.page-link");
        if (pageLink && pageLink.dataset.page) {
            e.preventDefault();
            const page = parseInt(pageLink.dataset.page);
            const pageSize = parseInt(document.getElementById("pageSizeSelect").value);
            loadBooks(page, pageSize);
            return;
        }
    });

    /* ================= UPDATE FORM SUBMIT ================= */
    updateForm?.addEventListener("submit", function (e) {
        e.preventDefault();

        const id = parseInt(updateBookIdInput.value);
        const title = updateTitleInput.value.trim();

        if (!title) {
            showMessage("Title is required", false);
            return;
        }

        fetch(`/Book/UpdateBook/${id}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Id: id, Title: title })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    loadBooks(getCurrentPage(), getCurrentPageSize());
                    closeModal(updateModalEl);
                    showMessage(data.message, true);
                } else {
                    showMessage(data.message, false);
                }
            })
            .catch(err => showMessage("Unexpected error: " + err, false));
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
    if (pageSizeSelect) {
        pageSizeSelect.addEventListener("change", function () {
            loadBooks(1, parseInt(this.value)); // Reset to page 1
        });
    }

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
                if (newTable && oldTable) {
                    oldTable.replaceWith(newTable);
                }

                // Replace pagination 
                const newPagination = doc.querySelector("nav[aria-label='Books pagination']");
                const oldPagination = document.querySelector("nav[aria-label='Books pagination']");
                if (newPagination && oldPagination) {
                    oldPagination.replaceWith(newPagination);
                }

                // Update currentPage hidden input
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
        return parseInt(document.getElementById("pageSizeSelect")?.value || 5);
    }

});
