(() => {
    const form = document.querySelector("form[data-roomtype-form='true']");
    if (!form) return;

    const fileInput = document.getElementById("imageFileInput");
    const imagePathInput = document.getElementById("imagePathInput");
    const previewWrap = document.getElementById("imagePreviewWrap");
    const previewImage = document.getElementById("imagePreview");
    const statusEl = document.getElementById("imageUploadStatus");
    const submitBtn = form.querySelector("input[type='submit'],button[type='submit']");
    const tokenInput = form.querySelector("input[name='__RequestVerificationToken']");

    if (!fileInput || !imagePathInput || !statusEl || !tokenInput || !window.roomTypeUploadConfig) return;

    const uploadUrl = window.roomTypeUploadConfig.uploadUrl;
    const maxSizeBytes = Number(window.roomTypeUploadConfig.maxSizeBytes || 2097152);
    const allowedTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    const setStatus = (message, type) => {
        statusEl.className = "form-text";
        if (type === "error") statusEl.classList.add("text-danger");
        else if (type === "success") statusEl.classList.add("text-success");
        else statusEl.classList.add("text-muted");
        statusEl.textContent = message || "";
    };

    const setSubmitting = (isSubmitting) => {
        if (!submitBtn) return;
        submitBtn.disabled = isSubmitting;
    };

    const showPreview = (src) => {
        if (!previewImage || !previewWrap) return;
        previewImage.src = src;
        previewWrap.style.display = "block";
    };

    const readFileAsDataUrl = (file) => new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(new Error("Khong the doc file anh."));
        reader.readAsDataURL(file);
    });

    fileInput.addEventListener("change", async () => {
        const file = fileInput.files && fileInput.files.length > 0 ? fileInput.files[0] : null;
        if (!file) {
            setStatus("", "normal");
            return;
        }

        if (!allowedTypes.includes(file.type)) {
            fileInput.value = "";
            setStatus("Chi chap nhan anh JPG, JPEG, PNG, GIF hoac WEBP.", "error");
            return;
        }

        if (file.size > maxSizeBytes) {
            fileInput.value = "";
            setStatus("Kich thuoc anh toi da la 2MB.", "error");
            return;
        }

        try {
            setSubmitting(true);
            setStatus("Dang tai anh len...", "normal");

            const dataUrl = await readFileAsDataUrl(file);
            showPreview(dataUrl);

            const response = await fetch(uploadUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": tokenInput.value
                },
                credentials: "same-origin",
                body: JSON.stringify({
                    dataUrl,
                    fileName: file.name
                })
            });

            let data = null;
            try {
                data = await response.json();
            } catch (_ignored) {
                data = null;
            }

            if (!response.ok || !data || !data.path) {
                throw new Error((data && data.error) || "Tai anh that bai. Vui long thu lai.");
            }

            imagePathInput.value = data.path;
            showPreview(data.path);
            setStatus("Tai anh thanh cong.", "success");
        } catch (error) {
            fileInput.value = "";
            setStatus(error.message || "Tai anh that bai. Vui long thu lai.", "error");
        } finally {
            setSubmitting(false);
        }
    });
})();
