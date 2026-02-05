const Api = (() => {
  const base = "";

  function token() {
    return localStorage.getItem("epoka_token") || "";
  }

  function setSession(session) {
    localStorage.setItem("epoka_token", session.token);
    localStorage.setItem("epoka_email", session.email);
    localStorage.setItem("epoka_fullName", session.fullName);
    localStorage.setItem("epoka_isAdmin", session.isAdmin ? "1" : "0");
  }

  function clearSession() {
    localStorage.removeItem("epoka_token");
    localStorage.removeItem("epoka_email");
    localStorage.removeItem("epoka_fullName");
    localStorage.removeItem("epoka_isAdmin");
  }

  function session() {
    const t = token();
    if (!t) return null;
    return {
      token: t,
      email: localStorage.getItem("epoka_email") || "",
      fullName: localStorage.getItem("epoka_fullName") || "",
      isAdmin: (localStorage.getItem("epoka_isAdmin") || "0") === "1"
    };
  }

  async function request(path, { method="GET", body=null, auth=false } = {}) {
    const headers = { "Content-Type": "application/json" };
    if (auth) {
      const t = token();
      if (t) headers["Authorization"] = `Bearer ${t}`;
    }
    const res = await fetch(`${base}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : null
    });
    if (res.status === 204) return null;
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) {
      const msg = data && (data.message || data.title) ? (data.message || data.title) : "Request failed";
      throw new Error(msg);
    }
    return data;
  }

  return {
    session,
    setSession,
    clearSession,
    getCategories: () => request("/api/categories"),
    getProducts: (params) => {
      const q = new URLSearchParams(params || {}).toString();
      return request(`/api/products${q ? `?${q}` : ""}`);
    },
    getProduct: (id) => request(`/api/products/${id}`),
    register: (payload) => request("/api/auth/register", { method:"POST", body: payload }),
    login: (payload) => request("/api/auth/login", { method:"POST", body: payload }),
    createOrder: (payload) => request("/api/orders", { method:"POST", body: payload, auth:true }),
    myOrders: () => request("/api/orders/mine", { auth:true }),
    adminAllOrders: () => request("/api/orders", { auth:true }),
    adminCreateCategory: (payload) => request("/api/categories", { method:"POST", body: payload, auth:true }),
    adminDeleteCategory: (id) => request(`/api/categories/${id}`, { method:"DELETE", auth:true }),
    adminCreateProduct: (payload) => request("/api/products", { method:"POST", body: payload, auth:true }),
    adminUpdateProduct: (id,payload) => request(`/api/products/${id}`, { method:"PUT", body: payload, auth:true }),
    adminDeleteProduct: (id) => request(`/api/products/${id}`, { method:"DELETE", auth:true }),
    adminUpdateOrderStatus: (id,status) => request(`/api/orders/${id}/status`, { method:"PUT", body: status, auth:true })
  };
})();

const Ui = (() => {
  let toastEl;

  function init() {
    toastEl = document.querySelector(".toast");
    renderNav();
  }

  function toast(title, message) {
    if (!toastEl) return;
    toastEl.innerHTML = `<div><strong>${escapeHtml(title)}</strong></div><div class="muted" style="margin-top:4px">${escapeHtml(message)}</div>`;
    toastEl.classList.add("show");
    setTimeout(() => toastEl.classList.remove("show"), 3200);
  }

  function escapeHtml(s) {
    return (s || "").replace(/[&<>"']/g, c => ({ "&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#039;" }[c]));
  }

  function money(v) {
    return new Intl.NumberFormat(undefined, { style:"currency", currency:"EUR" }).format(v);
  }

  function cart() {
    const raw = localStorage.getItem("epoka_cart");
    return raw ? JSON.parse(raw) : [];
  }

  function setCart(items) {
    localStorage.setItem("epoka_cart", JSON.stringify(items));
    renderNav();
  }

  function cartCount() {
    return cart().reduce((a,b) => a + (b.quantity || 0), 0);
  }

  function addToCart(product, qty) {
    const items = cart();
    const existing = items.find(i => i.productId === product.id);
    if (existing) existing.quantity += qty;
    else items.push({ productId: product.id, quantity: qty, name: product.name, price: product.price, imageUrl: product.imageUrl });
    setCart(items);
    toast("Added to cart", `${product.name} (${qty})`);
  }

  function removeFromCart(productId) {
    const items = cart().filter(i => i.productId !== productId);
    setCart(items);
  }

  function updateQty(productId, qty) {
    const items = cart();
    const item = items.find(i => i.productId === productId);
    if (!item) return;
    item.quantity = Math.max(1, qty);
    setCart(items);
  }

  function clearCart() {
    localStorage.removeItem("epoka_cart");
    renderNav();
  }

  function renderNav() {
    const sess = Api.session();
    const account = document.querySelector("[data-account]");
    const admin = document.querySelector("[data-admin]");
    const cartBadge = document.querySelector("[data-cartcount]");
    if (cartBadge) cartBadge.textContent = String(cartCount());

    if (account) {
      if (sess) {
        account.innerHTML = `<span class="badge">${escapeHtml(sess.fullName || sess.email)}</span>
          <button class="btn secondary" id="logoutBtn">Logout</button>`;
        const btn = document.getElementById("logoutBtn");
        btn.onclick = () => { Api.clearSession(); renderNav(); toast("Signed out", "You have been logged out."); };
      } else {
        account.innerHTML = `<a class="btn secondary" href="/login.html">Login</a>
          <a class="btn" href="/register.html">Create account</a>`;
      }
    }

    if (admin) admin.style.display = sess && sess.isAdmin ? "block" : "none";
  }

  return { init, toast, money, cart, setCart, addToCart, removeFromCart, updateQty, clearCart, escapeHtml };
})();

document.addEventListener("DOMContentLoaded", Ui.init);
