import React, { useState } from 'react';
import { Form, Button, Card, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';

function Register() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleRegister = (e) => {
    e.preventDefault();
    if (password !== confirmPassword) {
      setError("Passwords don't match");
      return;
    }
    // Simulated registration
    navigate('/login');
  };

  return (
    <div className="d-flex justify-content-center align-items-center vh-100 bg-light">
      <Card style={{ width: '25rem', padding: '2rem' }}>
        <h3 className="text-center mb-4">Register</h3>
        {error && <Alert variant="danger">{error}</Alert>}
        <Form onSubmit={handleRegister}>
          <Form.Group controlId="email" className="mb-3">
            <Form.Label>Email</Form.Label>
            <Form.Control 
              type="email" 
              value={email}
              onChange={(e) => setEmail(e.target.value)} 
              placeholder="Enter your email" 
              required 
            />
          </Form.Group>

          <Form.Group controlId="password" className="mb-3">
            <Form.Label>Password</Form.Label>
            <Form.Control 
              type="password" 
              value={password}
              onChange={(e) => setPassword(e.target.value)} 
              placeholder="Enter password" 
              required 
            />
          </Form.Group>

          <Form.Group controlId="confirmPassword" className="mb-3">
            <Form.Label>Confirm Password</Form.Label>
            <Form.Control 
              type="password" 
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)} 
              placeholder="Confirm password" 
              required 
            />
          </Form.Group>

          <Button type="submit" className="w-100">Register</Button>
        </Form>

        <p className="text-center mt-3">
          Already have an account? <a href="/login">Login</a>
        </p>
      </Card>
    </div>
  );
}

export default Register;