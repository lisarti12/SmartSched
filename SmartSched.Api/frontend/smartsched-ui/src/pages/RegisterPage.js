import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";

export default function RegisterPage() {
    const navigate = useNavigate();
    const { login } = useAuth();

    const [form, setForm] = useState({
        firstName: "",
        lastName: "",
        email: "",
        password: "",
        confirmPassword: "",
        role: "Student"
    });

    const [fieldErrors, setFieldErrors] = useState({});
    const [serverMessage, setServerMessage] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);

    const validateForm = () => {
        const errors = {};

        if (!form.firstName.trim()) {
            errors.firstName = "First name is required.";
        } else if (form.firstName.trim().length < 2) {
            errors.firstName = "First name must be at least 2 characters.";
        }

        if (!form.lastName.trim()) {
            errors.lastName = "Last name is required.";
        } else if (form.lastName.trim().length < 2) {
            errors.lastName = "Last name must be at least 2 characters.";
        }

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

        if (!form.confirmPassword.trim()) {
            errors.confirmPassword = "Please confirm your password.";
        } else if (form.password !== form.confirmPassword) {
            errors.confirmPassword = "Passwords do not match.";
        }

        if (!form.role) {
            errors.role = "Please select a role.";
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

            const response = await api.post("/auth/register", form);

            if (response.data.requiresApproval) {
                setSuccessMessage(
                    response.data.message || "Professor registration submitted. Awaiting approval by system admin."
                );

                setTimeout(() => {
                    navigate("/login");
                }, 3000);

                return;
            }

            login(response.data);

            setSuccessMessage("Registration successful. Redirecting in 3 seconds...");

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
                error.response?.data?.message || "Registration failed. Please try again."
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="center-page">
            <form className="card" onSubmit={handleSubmit} noValidate>
                <h2>Register</h2>

                {serverMessage && <p className="error">{serverMessage}</p>}
                {successMessage && <p className="success">{successMessage}</p>}

                <div className="form-group">
                    <input
                        name="firstName"
                        placeholder="First Name"
                        value={form.firstName}
                        onChange={handleChange}
                    />
                    {fieldErrors.firstName && (
                        <p className="field-error">{fieldErrors.firstName}</p>
                    )}
                </div>

                <div className="form-group">
                    <input
                        name="lastName"
                        placeholder="Last Name"
                        value={form.lastName}
                        onChange={handleChange}
                    />
                    {fieldErrors.lastName && (
                        <p className="field-error">{fieldErrors.lastName}</p>
                    )}
                </div>

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

                <div className="form-group">
                    <input
                        name="confirmPassword"
                        type="password"
                        placeholder="Confirm Password"
                        value={form.confirmPassword}
                        onChange={handleChange}
                    />
                    {fieldErrors.confirmPassword && (
                        <p className="field-error">{fieldErrors.confirmPassword}</p>
                    )}
                </div>

                <div className="form-group">
                    <select name="role" value={form.role} onChange={handleChange}>
                        <option value="Student">Student</option>
                        <option value="Professor">Professor</option>
                    </select>
                    {fieldErrors.role && (
                        <p className="field-error">{fieldErrors.role}</p>
                    )}
                </div>

                <button type="submit" disabled={isSubmitting}>
                    {isSubmitting ? "Registering..." : "Register"}
                </button>

                <p>
                    Already have an account? <Link to="/login">Login</Link>
                </p>
            </form>
        </div>
    );
}