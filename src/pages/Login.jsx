import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Form, Button, Card, Alert } from 'react-bootstrap';
import { useAuth } from '../context/AuthContext';
import BalanceModal from '../components/BalanceModal';

function Login() {
  const { login, balance } = useAuth();
  const [showBalance, setShowBalance] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      // Simulated API response
      const userData = {
        name: 'John Doe',
        email,
        balance: 120.50
      };
      login(userData);
      setShowBalance(true);
    } catch (err) {
      setError('Invalid login credentials');
    }
  };

  return (
    <div className="d-flex justify-content-center align-items-center vh-100 bg-light">
      <Card style={{ width: '25rem', padding: '2rem' }}>
        <h3 className="text-center mb-4">Login</h3>
        {error && <Alert variant="danger">{error}</Alert>}
        <Form onSubmit={handleLogin}>
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
              placeholder="Enter your password" 
              required 
            />
          </Form.Group>

          <Button type="submit" className="w-100">Login</Button>
        </Form>

        <p className="text-center mt-3">
          Don’t have an account? <a href="/register">Register</a>
        </p>

        <BalanceModal show={showBalance} onHide={() => { setShowBalance(false); navigate('/'); }} balance={balance} />
      </Card>
    </div>
  );
}

export default Login;