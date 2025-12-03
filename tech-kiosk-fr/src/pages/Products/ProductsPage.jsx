import React, { useEffect, useMemo, useState } from "react";
import "./ProductsPage.css";
import NavBar from "../../components/Navbar";

const PRODUCTS_API = "https://localhost:5016/api/product"; 
const PLACEHOLDER =
  "https://via.placeholder.com/600x600.png?text=No+Image";

export default function ProductsPage({ onAddToCart }) {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  // UI state
  const [searchText, setSearchText] = useState("");
  const [category, setCategory] = useState("All");
  const [priceMax, setPriceMax] = useState("");
  const [sortBy, setSortBy] = useState("featured");
  const [sidebarOpen, setSidebarOpen] = useState(true);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 9;

  // Fetch products
  useEffect(() => {
    let mounted = true;
    setLoading(true);
    fetch(PRODUCTS_API)
      .then((res) => res.json())
      .then((data) => {
        if (!mounted) return;
        const normalized = (data || []).map((p, i) => ({
          id: p.id ?? p._id ?? i,
          name: p.name ?? "Unnamed product",
          price: typeof p.price === "number" ? p.price : Number(p.price) || 0,
          category: p.category ?? "Uncategorized",
          imageUrl: p.imageUrl ?? p.image ?? "",
          description: p.description ?? "",
          quantity: typeof p.quantity === "number" ? p.quantity : p.qty ?? 1,
        }));
        setProducts(normalized);
      })
      .catch((err) => {
        console.error("Failed to fetch products:", err);
        setProducts([]);
      })
      .finally(() => setLoading(false));
    return () => {
      mounted = false;
    };
  }, []);

  // Categories
  const categories = useMemo(() => {
    const cats = new Set(products.map((p) => p.category || "Uncategorized"));
    return ["All", ...Array.from(cats)];
  }, [products]);

  // Filter + Sort
  const filtered = useMemo(() => {
    let list = products.slice();

    if (searchText.trim()) {
      const q = searchText.trim().toLowerCase();
      list = list.filter(
        (p) =>
          p.name.toLowerCase().includes(q) ||
          (p.description && p.description.toLowerCase().includes(q))
      );
    }

    if (category !== "All") {
      list = list.filter((p) => p.category === category);
    }

    const max = Number(priceMax);
    if (!Number.isNaN(max) && max > 0) {
      list = list.filter((p) => Number(p.price) <= max);
    }

    if (sortBy === "price-asc") {
      list.sort((a, b) => a.price - b.price);
    } else if (sortBy === "price-desc") {
      list.sort((a, b) => b.price - a.price);
    }

    return list;
  }, [products, searchText, category, priceMax, sortBy]);

  // Pagination
  const totalPages = Math.max(1, Math.ceil(filtered.length / itemsPerPage));

  useEffect(() => {
    if (currentPage > totalPages) setCurrentPage(1);
  }, [filtered.length, totalPages, currentPage]);

  const start = (currentPage - 1) * itemsPerPage;
  const paginated = filtered.slice(start, start + itemsPerPage);

  // Add to cart
  const handleAdd = (product) => {
    if (onAddToCart) onAddToCart(product);
    else alert(`${product.name} added to cart`);
  };

  // Image fallback
  const handleImgError = (e) => {
    if (e.target) e.target.src = PLACEHOLDER;
  };

  return (
    <>
      <NavBar />

      <div className="velvety-products-page">
        <div className="vp-container">

          {/* SIDEBAR */}
          <aside className={`vp-sidebar ${sidebarOpen ? "open" : "closed"}`}>
            <div className="vp-sidebar-header">
              <h3>Filters</h3>
              <button
                aria-label="Close filters"
                className="vp-sidebar-close"
                onClick={() => setSidebarOpen(false)}
              >
                x
              </button>
            </div>

            <div className="vp-filter-block">
              <label className="vp-label">Category</label>
              <div className="vp-category-list">
                {categories.map((c) => (
                  <button
                    key={c}
                    className={`vp-chip ${c === category ? "active" : ""}`}
                    onClick={() => {
                      setCategory(c);
                      setCurrentPage(1);
                    }}
                  >
                    {c}
                  </button>
                ))}
              </div>
            </div>

            <div className="vp-filter-block">
              <label className="vp-label">Max Price (R)</label>
              <input
                type="number"
                min="0"
                placeholder="No limit"
                value={priceMax}
                onChange={(e) => {
                  setPriceMax(e.target.value);
                  setCurrentPage(1);
                }}
                className="vp-input"
              />
            </div>

            <div className="vp-filter-block">
              <label className="vp-label">Sort</label>
              <select
                value={sortBy}
                onChange={(e) => setSortBy(e.target.value)}
                className="vp-select"
              >
                <option value="featured">Featured</option>
                <option value="price-asc">Price: Low → High</option>
                <option value="price-desc">Price: High → Low</option>
              </select>
            </div>

            <div className="vp-filter-block vp-reset">
              <button
                className="vp-reset-btn"
                onClick={() => {
                  setCategory("All");
                  setPriceMax("");
                  setSortBy("featured");
                  setSearchText("");
                  setCurrentPage(1);
                }}
              >
                Reset filters
              </button>
            </div>
          </aside>

          {/* MAIN */}
          <main className="vp-main">

            {/* Top bar */}
            <div className="vp-topbar">
              <div className="vp-top-left">
                <button
                  className="vp-sidebar-toggle"
                  onClick={() => setSidebarOpen((s) => !s)}
                >
                  ☰ Filters
                </button>

                <div className="vp-search-wrap">
                  <input
                    className="vp-search"
                    placeholder="Search products..."
                    value={searchText}
                    onChange={(e) => {
                      setSearchText(e.target.value);
                      setCurrentPage(1);
                    }}
                  />
                </div>
              </div>

              <div className="vp-top-right">
                <div className="vp-results">
                  {loading ? "Loading…" : `${filtered.length} products`}
                </div>
              </div>
            </div>

            {/* GRID */}
            <section className="vp-grid">
              {loading ? (
                Array.from({ length: itemsPerPage }).map((_, i) => (
                  <article key={i} className="vp-card vp-skeleton">
                    <div className="vp-card-media" />
                    <div className="vp-card-body">
                      <div className="vp-skel-line short" />
                      <div className="vp-skel-line" />
                      <div className="vp-skel-line small" />
                    </div>
                  </article>
                ))
              ) : paginated.length ? (
                paginated.map((p) => (
                  <article key={p.id} className="vp-card">
                    <div className="vp-card-media">
                      <img
                        src={p.imageUrl || PLACEHOLDER}
                        alt={p.name}
                        onError={handleImgError}
                        loading="lazy"
                      />
                    </div>

                    <div className="vp-card-body">
                      <div className="vp-card-meta">
                        <span className="vp-cat">{p.category}</span>
                      </div>

                      <h4 className="vp-title">{p.name}</h4>

                      <div className="vp-card-footer">
                        <div className="vp-price">
                          R {Number(p.price).toFixed(2)}
                        </div>

                        <button
                          className="vp-add-btn"
                          onClick={() => handleAdd(p)}
                          disabled={p.quantity <= 0}
                        >
                          {p.quantity > 0 ? "Add" : "Sold out"}
                        </button>
                      </div>
                    </div>
                  </article>
                ))
              ) : (
                <div className="vp-empty">No products match your filters.</div>
              )}
            </section>

            {/* PAGINATION */}
            <div className="vp-pagination">
              <button
                className="vp-page-btn"
                onClick={() => setCurrentPage((s) => Math.max(1, s - 1))}
                disabled={currentPage === 1}
              >
                ←
              </button>

              {Array.from({ length: totalPages }).map((_, i) => {
                const page = i + 1;
                const show =
                  page === 1 ||
                  page === totalPages ||
                  Math.abs(page - currentPage) <= 1;

                if (!show) {
                  return (
                    <span key={`gap-${i}`} className="vp-ellipsis">
                      …
                    </span>
                  );
                }

                return (
                  <button
                    key={page}
                    className={`vp-page-dot ${
                      page === currentPage ? "active" : ""
                    }`}
                    onClick={() => setCurrentPage(page)}
                  >
                    {page}
                  </button>
                );
              })}

              <button
                className="vp-page-btn"
                onClick={() => setCurrentPage((s) => Math.min(totalPages, s + 1))}
                disabled={currentPage === totalPages}
              >
                →
              </button>
            </div>
          </main>
        </div>
      </div>
    </>
  );
}
