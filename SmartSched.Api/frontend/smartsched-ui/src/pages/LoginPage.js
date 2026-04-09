import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";

export default function LoginPage() {
    const navigate = useNavigate();
    const { login } = useAuth();

    const [form, setForm] = useState({
        email: "",
        password: ""
    });

    const [fieldErrors, setFieldErrors] = useState({});
    const [serverMessage, setServerMessage] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);

    const validateForm = () => {
        const errors = {};

        if (!form.email.trim()) {
            errors.email = "Email is required.";
        } else if (!/^\S+@\S+\.\S+$/.test(form.email)) {
            errors.email = "Please enter a valid email address.";
        }

        if (!form.password.trim()) {
            errors.password = "Password is required.";
        } else if (form.password.length < 6) {
            errors.password = "Password must be at least 6 characters.";
        }

        return errors;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;

        setForm((prev) => ({
            ...prev,
            [name]: value
        }));

        setFieldErrors((prev) => ({
            ...prev,
            [name]: ""
        }));

        setServerMessage("");
        setSuccessMessage("");
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setServerMessage("");
        setSuccessMessage("");

        const errors = validateForm();
        setFieldErrors(errors);

        if (Object.keys(errors).length > 0) {
            return;
        }

        try {
            setIsSubmitting(true);

            const response = await api.post("/auth/login", form);
            login(response.data);

            setSuccessMessage("Login successful. Redirecting in 3 seconds...");

            setTimeout(() => {
                if (response.data.role === "Student") {
                    navigate("/student");
                } else if (response.data.role === "Professor") {
                    navigate("/professor");
                } else {
                    navigate("/admin");
                }
            }, 3000);
        } catch (error) {
            setServerMessage(
                error.response?.data?.message || "Login failed. Please try again."
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="center-page">
            <form className="card" onSubmit={handleSubmit} noValidate>
                <h2>Login</h2>

                {serverMessage && <p className="error">{serverMessage}</p>}
                {successMessage && <p className="success">{successMessage}</p>}

                <div className="form-group">
                    <input
                        name="email"
                        type="email"
                        placeholder="Email"
                        value={form.email}
                        onChange={handleChange}
                    />
                    {fieldErrors.email && (
                        <p className="field-error">{fieldErrors.email}</p>
                    )}
                </div>

                <div className="form-group">
                    <input
                        name="password"
                        type="password"
                        placeholder="Password"
                        value={form.password}
                        onChange={handleChange}
                    />
                    {fieldErrors.password && (
                        <p className="field-error">{fieldErrors.password}</p>
                    )}
                </div>

                <button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? "Logging in..." : "Login"}
                </button>

                <p>
                    Don’t have an account? <Link to="/register">Register</Link>
                </p>
            </form>
        </div>
    );
}