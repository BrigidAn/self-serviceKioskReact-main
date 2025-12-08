import React, { useEffect, useMemo, useState } from "react";
import "./ProductsPage.css";
import NavBar from "../../components/Navbar";
import { toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const PRODUCTS_API = "https://localhost:5016/api/product";
const PLACEHOLDER = "https://via.placeholder.com/600x600.png?text=No+Image";
const CART_API = "https://localhost:5016/api/cart";

export default function ProductsPage() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [cartCount, setCartCount] = useState(0);

  // UI state
  const [searchText, setSearchText] = useState("");
  const [category, setCategory] = useState("All");
  const [priceMax, setPriceMax] = useState("");
  const [sortBy, setSortBy] = useState("featured");

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 5;

  const token = localStorage.getItem("token");
  // Fetch products
  useEffect(() => {
    let mounted = true;
    const fetchProducts = async () => {
      setLoading(true);
      try {
        const res = await fetch(PRODUCTS_API);
        const data = await res.json();

        if (!mounted) return;

      const availableProducts = (data || [])
      .filter(p => (typeof p.quantity === "number" ? p.quantity : Number(p.qty)) > 0)
      .map(p => ({
        id: p.id ?? p._id ?? p.productId, // <--- make sure this matches backend
        name: p.name ?? "Unnamed product",
        price: Number(p.price) || 0,
        category: p.category ?? "Uncategorized",
        imageUrl: p.imageUrl ?? p.image ?? "",
        description: p.description ?? "",
        quantity: typeof p.quantity === "number" ? p.quantity : p.qty ?? 1,
      }));


      setProducts(availableProducts);

        // Fetch cart to get count
        const cartRes = await fetch(CART_API, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (cartRes.ok) {
          const cartData = await cartRes.json();
          setCartCount(cartData.items?.length || 0);
        }
      } catch (err) {
        console.error(err);
        toast.error("Failed to fetch products or cart");
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
    return () => { mounted = false; };
  }, [token]);

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
 const handleAdd = async (product) => {
    try {
      const res = await fetch(`${CART_API}/add`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ productId: product.id, quantity: 1 }),
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || "Failed to add item to cart");
      }

      // Decrease quantity in frontend
      setProducts((prev) =>
        prev.map((p) =>
          p.id === product.id ? { ...p, quantity: p.quantity - 1 } : p
        )
      );

      setCartCount((c) => c + 1);
      toast.success(`${product.name} added to cart`);
    } catch (err) {
      console.error(err);
      toast.error(err.message);
    }
  };


  // Image fallback
  const handleImgError = (e) => {
    if (e.target) e.target.src = PLACEHOLDER;
  };

  return (
    <>
      <NavBar cartCount={cartCount} />

      <div className="velvety-products-page">
        <div className="vp-container">


          {/* MAIN */}
          <main className="vp-main">

          <div className="floating-bg">
            <div className="circle c1"></div>
            <div className="circle c2"></div>
            <div className="circle c3"></div>
            <div className="circle c4"></div>
          </div>

            {/* FUTURISTIC HERO */}
            <section className="vp-hero">
              <div className="vp-hero-glow"></div>

              <h1 className="vp-hero-title">
                Explore the Future of Robotics
              </h1>

              <p className="vp-hero-subtitle">
                Smart machines. Intelligent accessories. Designed to enhance your world.
              </p>

              <div className="vp-hero-floating">
                <div className="float-shape fs1"></div>
                <div className="float-shape fs2"></div>
                <div className="float-shape fs3"></div>
              </div>
            </section>


            {/* Top bar */}
            <div className="vp-topbar">
              <div className="vp-top-left">


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
                  {loading ? "Loading…" : `${filtered.length} Products `}
                </div>
              </div>
            </div>

            {/* CATEGORY STRIP */}
            <div className="vp-category-strip">
              {categories.map((c) => (
                <button
                  key={c}
                  className={`vp-chip ${category === c ? "active" : ""}`}
                  onClick={() => {
                    setCategory(c);
                    setCurrentPage(1);
                  }}
                >
                  {c}
                </button>
              ))}
            </div>

            {/* FILTER & SORT BAR */}
<div className="vp-filter-sort">
  <div className="vp-filter-group">
    <label>Max Price:</label>
    <input
      type="number"
      placeholder="R Max"
      value={priceMax}
      onChange={(e) => setPriceMax(e.target.value)}
    />
  </div>

        <div className="vp-filter-group">
          <label>Sort By:</label>
          <select
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="featured">Featured</option>
            <option value="price-asc">Price: Low → High</option>
            <option value="price-desc">Price: High → Low</option>
          </select>
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

                      <p className="vp-desc">
                        {p.description?.slice(0, 60) || "No description"}...
                      </p>

                      <div className="vp-qty">In stock: {p.quantity}</div>

                      <div className="vp-card-footer">
                        <div className="vp-price">R {Number(p.price).toFixed(2)}</div>

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
