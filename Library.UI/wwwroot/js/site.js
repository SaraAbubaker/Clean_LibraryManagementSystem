document.addEventListener("DOMContentLoaded", function () {

    // 🔐 EXIT if not on UserType page
    const userTypePage = document.getElementById("usertype-page");
    if (!userTypePage) return;

    /* ================= COMMON ================= */

    const actionMessage = document.getElementById("actionMessage");

    function showMessage(message, success) {
        actionMessage.textContent = message;
        actionMessage.classList.remove("d-none", "alert-success", "alert-danger");
        actionMessage.classList.add(success ? "alert-success" : "alert-danger");
    }

    /* ================= CREATE ================= */

    const createBtn = document.getElementById("createUserTypeBtn");
    if (createBtn) {
        createBtn.addEventListener("click", () => {
            window.location.href = "/UserType/CreateUserType";
        });
    }

    /* ================= UPDATE ================= */

    const updateModalEl = document.getElementById("updateModal");
    const updateForm = document.getElementById("updateForm");
    const updateRoleInput = document.getElementById("updateRole");
    const updateUserTypeIdInput = document.getElementById("updateUserTypeId");

    const updateModal = updateModalEl
        ? new bootstrap.Modal(updateModalEl)
        : null;

    document.querySelectorAll(".update-btn").forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.preventDefault();

            const row = this.closest("tr");
            const id = this.dataset.userTypeId;
            const role = row.querySelector(".role").textContent;

            updateRoleInput.value = role;
            updateUserTypeIdInput.value = id;

            updateModal.show();
        });
    });

    updateForm?.addEventListener("submit", function (e) {
        e.preventDefault();

        const id = parseInt(updateUserTypeIdInput.value);
        const role = updateRoleInput.value.trim();

        fetch("/UserType/UpdateUserType", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Id: id, Role: role })
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    document.querySelector(`#userTypeRow-${id} .role`).textContent = role;
                    showMessage(data.message, true);
                    updateModal.hide();
                } else {
                    showMessage(data.message, false);
                }
            })
            .catch(err => showMessage("Unexpected error: " + err, false));
    });

    /* ================= ARCHIVE ================= */

    const archiveModalEl = document.getElementById("archiveModal");
    const archiveForm = document.getElementById("archiveForm");

    archiveModalEl?.addEventListener("show.bs.modal", function (event) {
        const button = event.relatedTarget;
        document.getElementById("archiveUserTypeId").value =
            button.dataset.userTypeId;
    });

    archiveForm?.addEventListener("submit", function (e) {
        e.preventDefault();

        const id = parseInt(document.getElementById("archiveUserTypeId").value);

        fetch("/UserType/ArchiveUserType", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(id)
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    document.getElementById(`userTypeRow-${id}`)?.remove();
                    bootstrap.Modal.getInstance(archiveModalEl).hide();
                    showMessage(data.message, true);
                } else {
                    showMessage(data.message, false);
                }
            })
            .catch(err => showMessage("Unexpected error: " + err, false));
    });
});
