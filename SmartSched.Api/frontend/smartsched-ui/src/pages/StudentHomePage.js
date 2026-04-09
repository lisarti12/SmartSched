import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";
import NotificationBell from "../components/NotificationBell";

export default function StudentHomePage() {
    const { user, logout } = useAuth();
    const [courses, setCourses] = useState([]);
    const [message, setMessage] = useState("");
    const [unreadChatCount, setUnreadChatCount] = useState(0);

    useEffect(() => {
        loadCourses();
        loadUnreadChatCount();

        const interval = setInterval(() => {
            loadUnreadChatCount();
        }, 3000);

        return () => clearInterval(interval);
    }, []);

    const loadCourses = async () => {
        try {
            const response = await api.get("/student/courses");
            setCourses(response.data);
        } catch (error) {
            console.error(error);
            setMessage("Failed to load courses.");
        }
    };

    const loadUnreadChatCount = async () => {
        try {
            const response = await api.get("/chat/unread-total");
            setUnreadChatCount(response.data.unreadTotal || 0);
        } catch (error) {
            console.error("Failed to load unread chat count:", error);
        }
    };

    return (
        <div className="page-container">
            <div className="admin-header">
                <div>
                    <h2>Student Dashboard</h2>
                    <p>Welcome, {user?.fullName}</p>
                </div>

                <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
                    <Link to="/student/calendar">Calendar</Link>
                    <Link to="/student/availability">Availability</Link>
                    <Link to="/student/schedule">SmartSched</Link>

                    <Link to="/chat">
                        <button>
                            Open Chat{unreadChatCount > 0 ? ` (${unreadChatCount})` : ""}
                        </button>
                    </Link>

                    <NotificationBell />
                    <button onClick={logout}>Logout</button>
                </div>
            </div>

            {message && <p className="error">{message}</p>}

            <div className="admin-section">
                <h3>My Courses</h3>

                {courses.length === 0 ? (
                    <p>You are not enrolled in any courses yet.</p>
                ) : (
                    <div className="compact-list">
                        {courses.map((course) => (
                            <Link
                                key={course.courseClassId}
                                to={`/student/courses/${course.courseClassId}`}
                                className="compact-list-item"
                                style={{ textDecoration: "none", color: "inherit" }}
                            >
                                <div>
                                    <strong>{course.title}</strong>
                                    <div className="subtext">{course.semester}</div>
                                    <div className="subtext">{course.description}</div>
                                </div>
                            </Link>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}