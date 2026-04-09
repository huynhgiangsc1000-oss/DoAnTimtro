@section Scripts {
    <script>
        document.addEventListener("DOMContentLoaded", function () {

            // =========================
            // 1. TOAST CUSTOM
            // =========================
            function createToast(title, message, type = "success") {
                const toastContainer =
                    document.getElementById("toast-container") || createToastContainer();

                const toast = document.createElement("div");
                toast.className = `toast toast-${type}`;

                let iconClass = "ri-check-line";
                if (type === "error") iconClass = "ri-error-warning-line";
                if (type === "info") iconClass = "ri-information-line";

                toast.innerHTML = `
                    <div class="toast-icon"><i class="${iconClass}"></i></div>
                    <div class="toast-content">
                        <h4>${title}</h4>
                        <p>${message}</p>
                    </div>
                    <button class="toast-close"><i class="ri-close-line"></i></button>
                `;

                toastContainer.appendChild(toast);

                toast.querySelector(".toast-close").addEventListener("click", () => {
                    toast.style.animation = "fadeOut 0.3s forwards";
                    setTimeout(() => toast.remove(), 300);
                });

                setTimeout(() => {
                    if (toast.parentElement) {
                        toast.style.animation = "fadeOut 0.3s forwards";
                        setTimeout(() => toast.remove(), 300);
                    }
                }, 3000);
            }

            function createToastContainer() {
                const container = document.createElement("div");
        container.id = "toast-container";
        container.style.cssText =
        "position: fixed; bottom: 20px; right: 20px; z-index: 9999; display: flex; flex-direction: column; gap: 10px;";
        document.body.appendChild(container);

        const style = document.createElement("style");
        style.innerHTML = `
        .toast {
            background: white;
        border-radius: 12px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.12);
        padding: 15px 20px;
        display: flex;
        align-items: center;
        gap: 15px;
        min-width: 300px;
        border-left: 4px solid #1677ff;
                    }
        .toast-error {border - left - color: #ef4444; }
        .toast-info {border - left - color: #3b82f6; }
        .toast-icon i {font - size: 1.4rem; color: #1677ff; }
        .toast-error .toast-icon i {color: #ef4444; }
        .toast-info .toast-icon i {color: #3b82f6; }
        .toast-content h4 {margin: 0; font-size: 1rem; color: #111827; }
        .toast-content p {margin: 0; font-size: 0.9rem; color: #6b7280; }
        .toast-close {
            background: none;
        border: none;
        font-size: 1.1rem;
        color: #6b7280;
        cursor: pointer;
        margin-left: auto;
                    }
        @keyframes fadeOut {
            from {opacity: 1; transform: translateX(0); }
        to {opacity: 0; transform: translateX(20px); }
                    }
        `;
        document.head.appendChild(style);

        return container;
            }

        // =========================
        // 2. FAVORITE ROOM
        // =========================
        const tokenInput = document.querySelector('#favorite-form input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : "";
        const buttons = document.querySelectorAll(".favorite-btn");

            buttons.forEach(btn => {
                const roomId = btn.getAttribute("data-room-id");
        const icon = btn.querySelector("i");

        fetch(`/Customer/Favorites/IsFavorite?roomId=${roomId}`)
                    .then(res => res.json())
                    .then(data => {
                        if (data.success && data.isFavorite) {
            icon.classList.remove("bi-heart");
        icon.classList.add("bi-heart-fill", "text-danger");
                        }
                    })
                    .catch(err => console.error("IsFavorite error:", err));

        btn.addEventListener("click", function (e) {
            e.preventDefault();

        fetch("/Customer/Favorites/Toggle", {
            method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
        "RequestVerificationToken": token
                        },
        body: `roomId=${roomId}`
                    })
                    .then(res => {
                        if (res.status === 401) {
            createToast("Chưa đăng nhập", "Bạn cần đăng nhập để dùng chức năng này.", "error");
                            setTimeout(() => {
            window.location.href = "/Identity/Account/Login";
                            }, 1200);
        return null;
                        }
        return res.json();
                    })
                    .then(data => {
                        if (!data) return;

        if (data.success) {
                            if (data.isFavorite) {
            icon.classList.remove("bi-heart");
        icon.classList.add("bi-heart-fill", "text-danger");
        createToast("Đã lưu", "Phòng đã được thêm vào mục yêu thích.");
                            } else {
            icon.classList.remove("bi-heart-fill", "text-danger");
        icon.classList.add("bi-heart");
        createToast("Đã bỏ lưu", "Phòng đã được xóa khỏi mục yêu thích.", "info");
                            }
                        } else {
            createToast("Thông báo", data.message || "Không thể xử lý yêu cầu.", "error");
                        }
                    })
                    .catch(err => {
            console.error("Toggle favorite error:", err);
        createToast("Lỗi", "Có lỗi xảy ra.", "error");
                    });
                });
            });

        // =========================
        // 3. SEARCH BUTTON
        // =========================
        const searchBtn = document.querySelector(".search-btn");
        if (searchBtn) {
            searchBtn.addEventListener("click", function () {
                const icon = searchBtn.querySelector("i");
                if (icon) {
                    icon.style.transition = "transform 0.5s";
                    icon.style.transform = "rotate(360deg)";
                    setTimeout(() => icon.style.transform = "rotate(0)", 500);
                }
            });
            }

        // =========================
        // 4. NAVBAR SCROLL
        // =========================
        const navbar = document.getElementById("navbar");
        if (navbar) {
            window.addEventListener("scroll", () => {
                if (window.scrollY > 50) {
                    navbar.classList.add("scrolled");
                } else {
                    navbar.classList.remove("scrolled");
                }
            });
            }
        });
    </script>
}