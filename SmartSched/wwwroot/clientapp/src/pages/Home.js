import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import StudentHome from "./home/StudentHome";
import EmployerHome from "./home/EmployerHome";
import AdminHome from "./home/AdminHome";

export default function Home() {
    const navigate = useNavigate();

    const [role, setRole] = useState("");
    const [name, setName] = useState("");

    useEffect(() => {
        const r = localStorage.getItem("role");
        const n = localStorage.getItem("name");

        if (!r) {
            navigate("/login");
            return;
        }

        setRole(r);
        setName(n);
    }, []);

    const logout = () => {
        localStorage.clear();
        navigate("/login");
    };

    return (
        <div>
            <h2>SmartSched</h2>

            {role === "Student" && <StudentHome name={name} />}
            {role === "Employer" && <EmployerHome name={name} />}
            {role === "Admin" && <AdminHome name={name} />}

            <button onClick={logout}>Logout</button>
        </div>
    );
}
