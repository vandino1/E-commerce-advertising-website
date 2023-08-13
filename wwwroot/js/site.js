const scrollTopBtn = document.querySelector('.scroll-top-btn');

scrollTopBtn.addEventListener('click', () => {
    scrollToTop();
});

function scrollToTop() {
    const currentPosition = window.pageYOffset;
    if (currentPosition > 0) {
        window.requestAnimationFrame(scrollToTop);
        window.scrollTo(0, currentPosition - currentPosition / 10);
    }
}

window.addEventListener('scroll', () => {
    if (window.scrollY > 500) {
        scrollTopBtn.style.display = 'block';
    } else {
        scrollTopBtn.style.display = 'none';
    }
});

var slides = document.querySelectorAll('.slide');
var currentSlide = 0;
var slideInterval = setInterval(nextSlide, 5000);

function nextSlide() {
    slides[currentSlide].classList.remove('active');
    currentSlide = (currentSlide + 1) % slides.length;
    slides[currentSlide].classList.add('active');
}

function prevSlide() {
    slides[currentSlide].classList.remove('active');
    currentSlide = (currentSlide - 1 + slides.length) % slides.length;
    slides[currentSlide].classList.add('active');
}

var prev = document.querySelector('.prev');
var next = document.querySelector('.next');

prev.addEventListener('click', prevSlide);
next.addEventListener('click', nextSlide);



