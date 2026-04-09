import { useEffect, useState } from "react";
import api from "../api/api";

export default function NotificationBell() {
    const [notifications, setNotifications] = useState([]);
    const [open, setOpen] = useState(false);

    useEffect(() => {
        loadNotifications();
    }, []);

    const loadNotifications = async () => {
        try {
            const response = await api.get("/student/task-notifications");
            setNotifications(response.data);
        } catch (error) {
            console.error("Failed to load notifications:", error);
        }
    };

    const unreadCount = notifications.filter((n) => !n.isRead).length;

    const markAsRead = async (id) => {
        try {
            await api.put(`/student/task-notifications/${id}/read`);
            setNotifications((prev) =>
                prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
            );
        } catch (error) {
            console.error("Failed to mark notification as read:", error);
        }
    };

    return (
        <div style={{ position: "relative" }}>
            <button onClick={() => setOpen(!open)}>
                🔔 {unreadCount > 0 ? `(${unreadCount})` : ""}
            </button>

            {open && (
                <div
                    style={{
                        position: "absolute",
                        right: 0,
                        top: "40px",
                        width: "340px",
                        background: "white",
                        border: "1px solid #ddd",
                        borderRadius: "8px",
                        padding: "10px",
                        zIndex: 100
                    }}
                >
                    <h4>Task Notifications</h4>

                    {notifications.length === 0 ? (
                        <p>No notifications yet.</p>
                    ) : (
                        notifications.map((notification) => (
                            <div
                                key={notification.id}
                                style={{
                                    marginBottom: "12px",
                                    paddingBottom: "8px",
                                    borderBottom: "1px solid #eee"
                                }}
                            >
                                <strong>{notification.title}</strong>
                                <p>{notification.message}</p>

                                {!notification.isRead && (
                                    <button onClick={() => markAsRead(notification.id)}>
                                        Mark as read
                                    </button>
                                )}
                            </div>
                        ))
                    )}
                </div>
            )}
        </div>
    );
}