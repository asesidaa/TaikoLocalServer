// Favourites page: reorder rows by dragging the handle (mouse or touch) or with the keyboard on it.
// The row moves in the DOM while dragging; on release the DOM goes back to Blazor's order and the new order is
// reported to .NET, which renders it (so Blazor's view of the list never goes stale).

export function init(list, net) {
  if (!list || list._fav) return;
  list._fav = true;

  list.addEventListener("pointerdown", e => {
    const grip = e.target.closest("[data-fv-grip]");
    if (!grip || e.button) return;
    e.preventDefault();
    const li = grip.closest("li"), before = [...list.children];
    li.classList.add("relative", "z-10", "shadow-lg", "ring-2", "ring-primary-500/60");
    // On window, not pointer capture: moving the row in the DOM drops capture.
    const move = ev => {
      const rows = [...list.children].filter(r => r !== li);
      const after = rows.find(r => { const b = r.getBoundingClientRect(); return ev.clientY < b.top + b.height / 2; });
      if (after !== li.nextSibling) list.insertBefore(li, after || null);
    };
    const up = () => {
      removeEventListener("pointermove", move);
      removeEventListener("pointerup", up);
      removeEventListener("pointercancel", up);
      li.classList.remove("relative", "z-10", "shadow-lg", "ring-2", "ring-primary-500/60");
      const order = [...list.children].map(r => +r.dataset.id);
      for (const r of before) list.appendChild(r);   // back to Blazor's order; .NET renders the new one
      if (order.join() !== before.map(r => +r.dataset.id).join()) net.invokeMethodAsync("Reordered", order);
    };
    addEventListener("pointermove", move);
    addEventListener("pointerup", up);
    addEventListener("pointercancel", up);
  });

  // ↑/↓ move by one, Home/End to the ends; only these keys are taken from the handle (Tab still moves on).
  list.addEventListener("keydown", async e => {
    const grip = e.target.closest("[data-fv-grip]");
    if (!grip || !["ArrowUp", "ArrowDown", "Home", "End"].includes(e.key)) return;
    e.preventDefault();
    const id = +grip.closest("li").dataset.id;
    await net.invokeMethodAsync("MoveByKey", id, e.key);
    list.querySelector(`li[data-id="${id}"] [data-fv-grip]`)?.focus();
  });
}

// The add-a-song box: ↑/↓ pick a result rather than moving the caret.
export function combobox(input) {
  if (!input || input._fav) return;
  input._fav = true;
  input.addEventListener("keydown", e => { if (e.key === "ArrowUp" || e.key === "ArrowDown") e.preventDefault(); });
}
