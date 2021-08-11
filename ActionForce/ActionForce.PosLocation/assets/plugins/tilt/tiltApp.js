const tilt = $('.tilt-animate').tilt({
    maxTilt:        40,
    perspective:    2000,   // Transform perspective, the lower the more extreme the tilt gets.
    easing:         "cubic-bezier(.03,.98,.52,.99)",    // Easing on enter/exit.
    scale:          1,      // 2 = 200%, 1.5 = 150%, etc..
    speed:          600,    // Speed of the enter/exit transition.
    transition:     true,   // Set a transition on enter/exit.
});