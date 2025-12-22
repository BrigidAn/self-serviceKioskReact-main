import React, { useState } from "react";
import NavBar from "../../components/Navbar";
import "./SupportPage.css";

export default function SupportPage() {
  const [formData, setFormData] = useState({
    name: "",
    email: "",
    issue: "",
    message: "",
  });

  const handleChange = (e) =>
    setFormData({ ...formData, [e.target.name]: e.target.value });

  const handleSubmit = (e) => {
    e.preventDefault();
    alert("Support request submitted");
    setFormData({ name: "", email: "", issue: "", message: "" });
  };

  const faqs = [
    {
      question: "How do I set up my delivery robot?",
      answer: "Follow the quick start guide included in the package."
    },
    {
      question: "Can I return a VR headset?",
      answer: "Yes, returns are accepted within 14 days of purchase."
    },
    {
      question: "How to troubleshoot my restaurant robot?",
      answer: "Check the troubleshooting section in the manual or contact support."
    },
  ];

  return (
    <div className="support-page">
      <NavBar />

      <div className="support-container">
        <button className="back-btn" onClick={() => window.history.back()}>
          ← Back
        </button>

        <h2>Customer Support</h2>
        <div className="support-content">
          <section className="faq-section">
            <h3>Frequently Asked Questions</h3>
            {faqs.map((faq, i) => (
              <div className="faq-card" key={i}>
                <p className="question">{faq.question}</p>
                <p className="answer">{faq.answer}</p>
              </div>
            ))}
          </section>

          <section className="contact-section">
            <h3>Contact Support</h3>
            <form className="support-form" onSubmit={handleSubmit}>
              <input
                name="name"
                value={formData.name}
                onChange={handleChange}
                placeholder="Your name"
                required
              />
              <input
                name="email"
                type="email"
                value={formData.email}
                onChange={handleChange}
                placeholder="Your email"
                required
              />
              <input
                name="issue"
                value={formData.issue}
                onChange={handleChange}
                placeholder="Issue type (e.g., Robot, VR Headset)"
                required
              />
              <textarea
                name="message"
                value={formData.message}
                onChange={handleChange}
                placeholder="Describe your issue"
                required
              />

              <div className="submit-row">
                <button type="submit">Submit</button>
              </div>
            </form>
          </section>
        </div>
      </div>
    </div>
  );
}
