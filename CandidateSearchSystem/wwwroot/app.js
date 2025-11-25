function scrollToElement(id) {
    const el = document.getElementById(id);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

window.showModal = function (id) {
    const modalElement = document.getElementById(id);
    if (modalElement) {
        const myModal = new bootstrap.Modal(modalElement);
        myModal.show();
    }
};