import React from "react";
import styles from "./Features.module.css";
import icon1 from "../assets/feature-icons/icon1.png";
import icon2 from "../assets/feature-icons/icon2.png";
import icon3 from "../assets/feature-icons/icon3.png";

const featuresData = [
  { icon: icon1, title: "Fast Service", description: "Complete orders quickly with our intuitive interface." },
  { icon: icon2, title: "Interactive", description: "Engaging and easy-to-use self-service kiosks." },
  { icon: icon3, title: "Reliable", description: "Robust systems designed to minimize downtime." },
];

const Features = () => {
  return (
    <div className={styles.container}>
      {featuresData.map((feature, index) => (
        <div className={styles.card} key={index}>
          <img src={feature.icon} alt={feature.title} className={styles.icon} />
          <h3 className={styles.title}>{feature.title}</h3>
          <p className={styles.description}>{feature.description}</p>
        </div>
      ))}
    </div>
  );
};

export default Features;
