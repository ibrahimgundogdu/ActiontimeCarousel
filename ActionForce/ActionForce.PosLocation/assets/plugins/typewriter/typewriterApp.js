var app = document.getElementById('typewriter');
    var typewriter = new Typewriter(app, {
        start: true,
        delay: 20,
        deleteSpeed: 20,
    });
    typewriter.typeString('<span class="icon" data-swiper-parallax-y="-50" data-swiper-parallax-duration="1500"></span><b data-swiper-parallax-y="25" data-swiper-parallax-duration="1750">Yeni Nesil Teknolojiler</b> <br> <span class="end-text">ile Geleceği İnşa Ediyoruz.</span>')
        .pauseFor(1000)
        .start();