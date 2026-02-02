import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

export default function Login() {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        email: "",
        password: ""
    });

    const [message, setMessage] = useState("");
    const [errors, setErrors] = useState({});

    const submit = async (e) => {
        e.preventDefault();

        setMessage("");
        setErrors({});

        // --- BASIC CLIENT VALIDATION ---
        const clientErrors = {};

        if (!form.email.includes("@"))
            clientErrors.email = "Please enter a valid email";

        if (!form.password)
            clientErrors.password = "Password is required";

        if (Object.keys(clientErrors).length > 0) {
            setErrors(clientErrors);
            return;
        }

        try {
            const res = await fetch("https://localhost:7243/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(form)
            });

            const data = await res.json();

            if (!res.ok) {
                // 👇 REAL WEBSITE STYLE MESSAGE
                setMessage(data.message || "Invalid email or password");
                return;
            }

            setMessage("Login successful!");

            // store user info from backend
            localStorage.setItem("role", data.role);
            localStorage.setItem("name", data.name);

            setTimeout(() => {
                navigate("/home");
            }, 1000);


            // optional
            if (data.token)
                localStorage.setItem("token", data.token);

            // go to YOUR homepage
            setTimeout(() => {
                navigate("/home");
            }, 1000);

        } catch (err) {
            setMessage("Cannot connect to server");
        }
    };

    return (
        <div className="auth-container">
            <form onSubmit={submit} className="auth-box">
                <h2>Login</h2>

                {message && <div className="message">{message}</div>}

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

                <button>Login</button>

                <p>
                    Don’t have an account?{" "}
                    <Link to="/register">Register here</Link>
                </p>
            </form>
        </div>
    );
}
