document.addEventListener("click", function (event) {
    if (event.target instanceof Element && event.target.closest(".nav-scrollable")) {
        var toggler = document.querySelector(".navbar-toggler");
        if (toggler) {
            toggler.click();
        }
    }
});
