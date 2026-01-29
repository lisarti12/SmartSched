import { useState } from "react";

function Register() {
    const [form, setForm] = useState({
        fullName: "",
        email: "",
        password: ""
    });

    const submit = async (e) => {
        e.preventDefault();

        await fetch("/api/auth/register", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(form)
        });
    };

    return (
        <form onSubmit={submit}>
            <h2>Register</h2>

            <input
                placeholder="Full Name"
                value={form.fullName}
                onChange={(e) => setForm({ ...form, fullName: e.target.value })}
            />

            <input
                placeholder="Email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
            />

            <input
                type="password"
                placeholder="Password"
                value={form.password}
                onChange={(e) => setForm({ ...form, password: e.target.value })}
            />

            <button type="submit">Register</button>
        </form>
    );
}

export default Register;
