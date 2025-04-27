import React from "react";
import { Link } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import Button from "../components/common/Button";
import "./HomePage.css";

const HomePage = () => {
  return (
    <PageLayout>
      <section className="hero">
        <div className="hero-content">
          <div className="hero-text">
            <h1 className="hero-main">
              What is poetry? Poetry is the language of the soul.
            </h1>
            <p>
              " What is poetry? And this is what it is: the union of two words
              that no one suspected they could join, and that, joined together,
              they would express a new mystery whenever they were uttered. "
            </p>
          </div>
          <div className="hero-image">
            <img src="/assets/images/Byron.jpg" alt="Lord Byron" />
          </div>
        </div>
      </section>

      <section className="section-how">
        <div className="container">
          <span className="subheading">What you can do</span>
          <h2 className="heading-secondary">
            Share your poems and get inspired in 3 easy steps
          </h2>
        </div>

        <div className="container grid grid--2-cols grid--center-v">
          {/* STEP 01 */}
          <div className="step-text-box">
            <p className="step-number">01</p>
            <h3 className="heading-tertiary">Share a poem</h3>
            <p className="step-description">
              Share your poems or find inspiration in the works of other poets.
              Our website makes it easy to publish your work and share it with
              the world.
            </p>
          </div>

          <div className="step-img-box">
            <img
              src="/assets/images/Write.png"
              className="step-img"
              alt="Writing poem screen"
            />
          </div>

          {/* STEP 02 */}
          <div className="step-img-box">
            <img
              src="/assets/images/Reading.png"
              className="step-img"
              alt="Reading poems screen"
            />
          </div>
          <div className="step-text-box">
            <p className="step-number">02</p>
            <h3 className="heading-tertiary">Read and discuss</h3>
            <p className="step-description">
              Read other authors' poems, discuss them in comments and make new
              friends. Be inspired by others' work, create new poems and share
              them in our community.
            </p>
          </div>

          {/* STEP 03 */}
          <div className="step-text-box">
            <p className="step-number">03</p>
            <h3 className="heading-tertiary">Join the Discussion</h3>
            <p className="step-description">
              Participate in poetry discussions, create topics on literature, or
              join existing conversations. Share your opinion and find
              like-minded people.
            </p>
          </div>
          <div className="step-img-box">
            <img
              src="/assets/images/Discussion.png"
              className="step-img"
              alt="Poetry discussion screen"
            />
          </div>
        </div>
      </section>

      <section className="section-reviews">
        <div className="container">
          <h2 className="heading-secondary">What our users are saying</h2>
          <div className="reviews-grid">
            <div className="review">
              <p className="review-text">
                "A wonderful platform for poets! Found lots of inspiration and
                new friends."
              </p>
              <p className="review-author">— John Doe</p>
            </div>
            <div className="review">
              <p className="review-text">
                "A very user-friendly site for publishing poetry. Easy to use
                and a cool community!"
              </p>
              <p className="review-author">— Taras Shevchenko</p>
            </div>
            <div className="review">
              <p className="review-text">
                "Reading poetry every day, finding new works and discussing them
                with others."
              </p>
              <p className="review-author">— Mikhail Lermontov</p>
            </div>
          </div>
        </div>
      </section>

      <section className="section-cta">
        <div className="container">
          <h2 className="heading-secondary">Join our community of poets</h2>
          <p className="cta-text">
            Publish your poems, read the work of others and find inspiration!
          </p>
          <div className="cta-buttons">
            <Button to="/cabinet" variant="primary" size="lg">
              Publish a poem
            </Button>
            <Button to="/register" variant="secondary" size="lg">
              Join
            </Button>
          </div>
        </div>
      </section>
    </PageLayout>
  );
};

export default HomePage;
