document.addEventListener("DOMContentLoaded", function () {

    // 🔐 EXIT if not on Books page
    const bookPage = document.getElementById("book-page");
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

    /* ================= CREATE ================= */

    const createBtn = document.getElementById("createBookBtn");
    if (createBtn) {
        createBtn.addEventListener("click", () => {
            window.location.href = "/Book/CreateBook"; // Adjust route as needed
        });
    }

    /* ================= UPDATE ================= */

    const updateModalEl = document.getElementById("updateModal");
    const updateForm = document.getElementById("updateForm");
    const updateTitleInput = document.getElementById("updateTitle");
    const updateBookIdInput = document.getElementById("updateBookId");

    if (updateModalEl) {
        const updateModal = new bootstrap.Modal(updateModalEl);

        document.querySelectorAll(".update-btn").forEach(btn => {
            btn.addEventListener("click", function (e) {
                e.preventDefault();

                const row = this.closest("tr");
                const id = this.dataset.bookId;
                const title = row.querySelector(".title").textContent;

                updateTitleInput.value = title;
                updateBookIdInput.value = id;

                updateModal.show();
            });
        });

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
                        document.querySelector(`#bookRow-${id} .title`).textContent = title;
                        showMessage(data.message, true);
                        closeModal(updateModalEl);
                    } else {
                        showMessage(data.message, false);
                    }
                })
                .catch(err => showMessage("Unexpected error: " + err, false));
        });
    }

    /* ================= ARCHIVE ================= */

    const archiveModalEl = document.getElementById("archiveModal");
    const archiveForm = document.getElementById("archiveForm");

    archiveModalEl?.addEventListener("show.bs.modal", function (event) {
        const button = event.relatedTarget;
        document.getElementById("archiveBookId").value =
            button.dataset.bookId;
    });

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
                    document.getElementById(`bookRow-${id}`)?.remove();
                    closeModal(archiveModalEl);
                    showMessage(data.message, true);
                } else {
                    showMessage(data.message, false);
                }
            })
            .catch(err => showMessage("Unexpected error: " + err, false));
    });

});
