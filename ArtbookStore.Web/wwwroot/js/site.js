"use strict";

// Carousel for the hero section on the homepage
document.addEventListener("DOMContentLoaded", function () {

    // Get all the slide elements from the DOM
    const hero = document.querySelector(".sf-hero");
    if (!hero) return;

    // The content container that will be animated during transitions
    const heroContent = hero.querySelector(".sf-hero-content");

    // The elements that will display the title, description, and image of each slide
    const elTitle = document.getElementById("sf-title");
    const elDesc = document.getElementById("sf-desc");
    const elImg = document.getElementById("sf-img");

    // The navigation buttons and dots for the carousel
    const prevBtn = hero.querySelector(".sf-prev");
    const nextBtn = hero.querySelector(".sf-next");
    const dots = Array.from(hero.querySelectorAll(".sf-dot"));

    const slideEls = Array.from(hero.querySelectorAll(".sf-slides .sf-slide"));

    // Basic checks to ensure all necessary elements are present before initializing the carousel
    if (!elTitle || !elDesc || !elImg) return;
    if (!prevBtn || !nextBtn) return;
    if (slideEls.length === 0) return;
    if (dots.length === 0) return;

    // We will only use as many slides as there are dots, to ensure proper navigation
    const count = Math.min(slideEls.length, dots.length);
    let current = 0;

    // Helper function to extract the title, description, and image URL from a slide element
    function getSlide(i) {
        const s = slideEls[i];
        return {
            title: s.getAttribute("data-title") || "",
            desc: s.getAttribute("data-desc") || "",
            img: s.getAttribute("data-img") || ""
        };
    }

    // Helper function to update the active state of the navigation dots based on the current slide index
    function setActiveDot(i) {
        for (let k = 0; k < dots.length; k++) {
            dots[k].classList.remove("is-active");
            dots[k].setAttribute("aria-selected", "false");
            dots[k].setAttribute("tabindex", "-1");
        }

        // Activate the current dot
        if (dots[i]) {
            dots[i].classList.add("is-active");
            dots[i].setAttribute("aria-selected", "true");
            dots[i].setAttribute("tabindex", "0");
        }
    }

    // The main function to render a slide based on its index, with a simple fade transition
    function render(i) {

        const s = getSlide(i);

        // Animation start
        heroContent.classList.add("is-transitioning");

        // After a short delay to allow the fade-out animation to play, update the content and fade back in
        setTimeout(() => {

            elTitle.textContent = s.title;
            elDesc.textContent = s.desc;
            elImg.src = s.img;
            elImg.alt = s.title || "Artbook cover";

            setActiveDot(i);

            heroContent.classList.remove("is-transitioning");

        }, 180);
    }

    // Functions to navigate to the next and previous slides, wrapping around at the ends
    function next() {
        current = (current + 1) % count;
        render(current);
    }

    function prev() {
        current = (current - 1 + count) % count;
        render(current);
    }

    // Attach event listeners to the navigation buttons and dots, as well as keyboard navigation for accessibility
    nextBtn.addEventListener("click", next);
    prevBtn.addEventListener("click", prev);

    // Set up the dots with appropriate ARIA attributes and event listeners for click and keyboard interaction
    for (let i = 0; i < count; i++) {

        dots[i].setAttribute("aria-label", `Slide ${i + 1}`);

        dots[i].addEventListener("click", function () {
            current = i;
            render(current);
        });

        dots[i].addEventListener("keydown", function (e) {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                current = i;
                render(current);
            }
        });
    }

    // Keyboard navigation for the entire carousel, allowing users to use arrow keys to navigate between slides
    document.addEventListener("keydown", function (e) {
        if (e.key === "ArrowRight") next();
        if (e.key === "ArrowLeft") prev();
    });

    // If the image fails to load, attempt to load the first slide's image as a fallback
    elImg.addEventListener("error", function () {
        const fallback = getSlide(0).img;
        if (fallback && elImg.src !== fallback) {
            elImg.src = fallback;
        }
    });

    // Initial render of the first slide
    render(current);
});
