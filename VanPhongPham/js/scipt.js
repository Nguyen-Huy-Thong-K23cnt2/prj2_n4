document.addEventListener("DOMContentLoaded", () => {
  const target = new Date("2025-09-30T23:59:59").getTime();
  const el = document.getElementById("countdown");

  function render() {
    let ms = target - new Date();
    if (ms <= 0) {
      el.textContent = "Hết giờ!";
      clearInterval(timer);
      return;
    }

    const week = 1000*60*60*24*7;
    const day  = 1000*60*60*24;
    const hour = 1000*60*60;
    const min  = 1000*60;

    const weeks   = Math.floor(ms / week); ms %= week;
    const days    = Math.floor(ms / day);  ms %= day;
    const hours   = Math.floor(ms / hour); ms %= hour;
    const minutes = Math.floor(ms / min);
    const seconds = Math.floor((ms % min) / 1000);

    el.innerHTML = `
      <div class="time-box"><b>${weeks}</b><span>Weeks</span></div>
      <div class="time-box"><b>${days}</b><span>Days</span></div>
      <div class="time-box"><b>${hours}</b><span>Hours</span></div>
      <div class="time-box"><b>${minutes}</b><span>Min</span></div>
      <div class="time-box"><b>${seconds}</b><span>Sec</span></div>`;
  }

  render();
  const timer = setInterval(render, 1000);
});

