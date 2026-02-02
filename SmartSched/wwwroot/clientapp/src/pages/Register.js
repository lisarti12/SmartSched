import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

export default function Register() {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
        role: "Student"
    });

    const [message, setMessage] = useState("");
    const [errors, setErrors] = useState({});

    // Client-side validation
    const validate = () => {
        const e = {};
        if (!form.fullName) e.fullName = "Name is required";
        if (!form.email.includes("@")) e.email = "Invalid email address";
        if (form.password.length < 6) e.password = "Password must be at least 6 characters";
        if (form.password !== form.confirmPassword) e.confirmPassword = "Passwords do not match";
        return e;
    };

    const submit = async (ev) => {
        ev.preventDefault();

        setMessage("");
        const clientErrors = validate();
        setErrors(clientErrors);

        if (Object.keys(clientErrors).length > 0) return;

        try {
            const res = await fetch("https://localhost:7243/api/auth/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(form)
            });


            const data = await res.json();

            if (!res.ok) {
                // Show backend validation errors or email already exists
                setErrors(data.errors || {});
                setMessage(data.message || "Registration failed");
                return;
            }

            // Registration successful
            setMessage("Registration successful! Redirecting to login...");

            // Clear form (optional)
            setForm({
                fullName: "",
                email: "",
                password: "",
                confirmPassword: "",
                role: "Student"
            });

            // Redirect to login after 1.5 seconds
            setTimeout(() => navigate("/login"), 1500);

        } catch (err) {
            setMessage("Something went wrong. Please try again.");
        }
    };

    return (
        <div className="auth-container">
            <form onSubmit={submit} className="auth-box">
                <h2>Create Account</h2>

                {message && <div className="message">{message}</div>}

                <input
                    placeholder="Full Name"
                    value={form.fullName}
                    onChange={e => setForm({ ...form, fullName: e.target.value })}
                />
                {errors.fullName && <div className="error">{errors.fullName}</div>}

                <input
                    placeholder="Email"
                    value={form.email}
                    onChange={e => setForm({ ...form, email: e.target.value })}
                />
                {errors.email && <div className="error">{errors.email}</div>}

                <input
                    type="password"
                    placeholder="Password"
                    value={form.password}
                    onChange={e => setForm({ ...form, password: e.target.value })}
                />
                {errors.password && <div className="error">{errors.password}</div>}

                <input
                    type="password"
                    placeholder="Confirm Password"
                    value={form.confirmPassword}
                    onChange={e => setForm({ ...form, confirmPassword: e.target.value })}
                />
                {errors.confirmPassword && <div className="error">{errors.confirmPassword}</div>}

                <select
                    value={form.role}
                    onChange={e => setForm({ ...form, role: e.target.value })}
                >
                    <option value="Student">Student</option>
                    <option value="Employer">Employer / Professor</option>
                </select>

                <button>Register</button>

                <p>
                    Already have an account? <Link to="/login">Login</Link>
                </p>
            </form>
        </div>
    );
}
