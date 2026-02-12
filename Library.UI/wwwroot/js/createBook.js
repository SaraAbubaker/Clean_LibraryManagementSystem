document.addEventListener("DOMContentLoaded", function () {

    function showSuccessMessage(message) {
        // Create a temporary alert at the top of the form
        const alertDiv = document.createElement("div");
        alertDiv.className = "alert alert-success alert-dismissible fade show mt-2";
        alertDiv.role = "alert";
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.querySelector("form").prepend(alertDiv);

        // Auto-dismiss after 3 seconds
        setTimeout(() => {
            alertDiv.classList.remove("show");
            alertDiv.classList.add("hide");
            alertDiv.remove();
        }, 3000);
    }

    // Add Author
    const addAuthorForm = document.getElementById("addAuthorForm");
    if (addAuthorForm) {
        addAuthorForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const nameInput = document.getElementById("newAuthorName");
            const name = nameInput.value.trim();
            if (!name) return;

            try {
                const response = await fetch("/Author/CreateAuthor", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ name })
                });

                if (response.ok) {
                    const data = await response.json();

                    // Add to dropdown
                    const select = document.querySelector("select[name='AuthorId']");
                    const option = new Option(data.name, data.id, true, true);
                    select.add(option);

                    // Close modal
                    bootstrap.Modal.getInstance(document.getElementById("addAuthorModal")).hide();

                    // Clear input
                    nameInput.value = "";

                    // Show success message
                    showSuccessMessage("Author added successfully!");
                } else {
                    const error = await response.text();
                    alert("Failed to add author: " + error);
                }
            } catch (err) {
                console.error(err);
                alert("Unexpected error while adding author.");
            }
        });
    }

    // Add Publisher
    const addPublisherForm = document.getElementById("addPublisherForm");
    if (addPublisherForm) {
        addPublisherForm.addEventListener("submit", async function (e) {
            e.preventDefault();

            const nameInput = document.getElementById("newPublisherName");
            const name = nameInput.value.trim();
            if (!name) return;

            try {
                const response = await fetch("/Publisher/CreatePublisher", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ name })
                });

                if (response.ok) {
                    const data = await response.json();

                    // Add to dropdown
                    const select = document.querySelector("select[name='PublisherId']");
                    const option = new Option(data.name, data.id, true, true);
                    select.add(option);

                    // Close modal
                    bootstrap.Modal.getInstance(document.getElementById("addPublisherModal")).hide();

                    // Clear input
                    nameInput.value = "";

                    // Show success message
                    showSuccessMessage("Publisher added successfully!");
                } else {
                    const error = await response.text();
                    alert("Failed to add publisher: " + error);
                }
            } catch (err) {
                console.error(err);
                alert("Unexpected error while adding publisher.");
            }
        });
    }

});
