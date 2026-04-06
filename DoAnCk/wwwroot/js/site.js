@section Scripts {
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const token = document.querySelector('#favorite-form input[name="__RequestVerificationToken"]').value;
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
                    });

        btn.addEventListener("click", function () {
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
                        alert("Bạn cần đăng nhập để dùng chức năng này.");
                        window.location.href = "/Identity/Account/Login";
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
                        } else {
                            icon.classList.remove("bi-heart-fill", "text-danger");
                            icon.classList.add("bi-heart");
                        }
                    } else {
                        alert(data.message);
                    }
                })
                .catch(err => {
                    console.error(err);
                    alert("Có lỗi xảy ra.");
                });
                });
            });
        });
    </script>
}