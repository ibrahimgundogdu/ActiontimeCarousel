$(function () {
  /* General Settings */
  const open = 'open';
  const $hamburger = $('.hamburger');
  const $closeMenu = $('.menu-overlay');
  $hamburger.on('click',function () {
    const $this = $(this);
    $this.toggleClass(open);
    $('body').toggleClass('body-hidden');
  });
  $closeMenu.on('click',function () {
    $hamburger.removeClass(open);
  });
  /*/ General Settings */

  /********************************************************************************* */

});
