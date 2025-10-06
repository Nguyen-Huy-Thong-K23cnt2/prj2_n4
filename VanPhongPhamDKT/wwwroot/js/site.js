document.addEventListener("DOMContentLoaded", () => {
    const target = new Date("2026-09-30T23:59:59").getTime();
    const el = document.getElementById("countdown");

    function render() {
        let ms = target - new Date();
        if (ms <= 0) {
            el.textContent = "Hết giờ!";
            clearInterval(timer);
            return;
        }

        const week = 1000 * 60 * 60 * 24 * 7;
        const day = 1000 * 60 * 60 * 24;
        const hour = 1000 * 60 * 60;
        const min = 1000 * 60;

        const weeks = Math.floor(ms / week); ms %= week;
        const days = Math.floor(ms / day); ms %= day;
        const hours = Math.floor(ms / hour); ms %= hour;
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
//---------------------------------------------------------------------------* /
// js đơn giản: hiển thị giá & lọc & sắp xếp
const fm = n => n.toLocaleString('vi-VN') + 'đ';
const min = document.getElementById('minPrice');
const max = document.getElementById('maxPrice');
const priceText = document.getElementById('priceText');
const grid = document.getElementById('productGrid');

function renderText() {
    const a = Math.min(+min.value, +max.value);
    const b = Math.max(+min.value, +max.value);
    priceText.textContent = `${fm(a)} — ${fm(b)}`;
}
min.addEventListener('input', renderText);
max.addEventListener('input', renderText);
renderText();

document.getElementById('btnFilter').addEventListener('click', () => {
    const lo = Math.min(+min.value, +max.value);
    const hi = Math.max(+min.value, +max.value);
    [...grid.children].forEach(c => {
        const p = +c.dataset.price;
        c.style.display = (p >= lo && p <= hi) ? '' : 'none';
    });
});

document.getElementById('sort').addEventListener('change', e => {
    const v = e.target.value;
    const cards = [...grid.children];
    const get = (c, k) => k === 'new' ? new Date(c.dataset.new).getTime() : +c.dataset[k];
    switch (v) {
        case 'asc': cards.sort((a, b) => a.dataset.price - b.dataset.price); break;
        case 'desc': cards.sort((a, b) => b.dataset.price - a.dataset.price); break;
        case 'pop': cards.sort((a, b) => get(b, 'pop') - get(a, 'pop')); break;
        case 'rate': cards.sort((a, b) => get(b, 'rate') - get(a, 'rate')); break;
        case 'new': cards.sort((a, b) => get(b, 'new') - get(a, 'new')); break;
        default: return; // giữ nguyên DOM
    }
    cards.forEach(c => grid.appendChild(c));
});
