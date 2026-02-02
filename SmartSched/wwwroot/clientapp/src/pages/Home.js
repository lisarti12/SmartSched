import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

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
        <div className="home-container">
            <h2>Welcome, {name}</h2>
            <p>Your role: {role}</p>

            {role === "Student" && <StudentHome />}
            {role === "Employer" && <EmployerHome />}
            {role === "Admin" && <AdminHome />}

            <button onClick={logout}>Logout</button>
        </div>
    );
}


function StudentHome() {
    return (
        <div className="panel">
            <h3>Student Dashboard</h3>

            <ul>
                <li>My Calendar</li>
                <li>Add Personal Event</li>
                <li>View Deadlines</li>
            </ul>
        </div>
    );
}

function EmployerHome() {
    return (
        <div className="panel">
            <h3>Employer / Professor Dashboard</h3>

            <ul>
                <li>Create Group Calendar</li>
                <li>Assign Tasks</li>
                <li>View Team Availability</li>
            </ul>
        </div>
    );
}

function AdminHome() {
    return (
        <div className="panel admin">
            <h3>Administrator Panel</h3>

            <ul>
                <li>Manage Users</li>
                <li>View All Calendars</li>
                <li>System Settings</li>
            </ul>
        </div>
    );
}
