import { useEffect, useState } from "react";
import api from "../api/api";
import { useAuth } from "../context/AuthContext";

export default function AdminHomePage() {
    const { user, logout } = useAuth();

    const [users, setUsers] = useState([]);
    const [pendingProfessors, setPendingProfessors] = useState([]);
    const [kpis, setKpis] = useState(null);
    const [message, setMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [userToDelete, setUserToDelete] = useState(null);

    const loadData = async () => {
        try {
            const [usersRes, pendingRes, kpisRes] = await Promise.all([
                api.get("/admin/users"),
                api.get("/admin/pending-professors"),
                api.get("/admin/kpis")
            ]);

            setUsers(usersRes.data);
            setPendingProfessors(pendingRes.data);
            setKpis(kpisRes.data);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to load admin data.");
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const approveProfessor = async (id) => {
        try {
            await api.put(`/admin/approve-professor/${id}`);
            setMessage("Professor approved successfully.");
            setErrorMessage("");
            loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to approve professor.");
            setMessage("");
        }
    };

    const declineProfessor = async (id) => {
        try {
            await api.delete(`/admin/decline-professor/${id}`);
            setMessage("Professor registration declined.");
            setErrorMessage("");
            loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to decline professor.");
            setMessage("");
        }
    };

    const changeRole = async (id, newRole) => {
        try {
            await api.put(`/admin/change-role/${id}`, { newRole });
            setMessage(`Role changed to ${newRole}.`);
            setErrorMessage("");
            loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to change role.");
            setMessage("");
        }
    };

    const confirmDeleteUser = (user) => {
        setUserToDelete(user);
    };

    const deleteUser = async () => {
        if (!userToDelete) return;

        try {
            await api.delete(`/admin/users/${userToDelete.id}`);
            setMessage("User deleted successfully.");
            setErrorMessage("");
            setUserToDelete(null);
            loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage(
                error.response?.data?.message || "Failed to delete user."
            );
            setMessage("");
            setUserToDelete(null);
        }
    };

    const cancelDelete = () => {
        setUserToDelete(null);
    };

    return (
        <div className="page-container">
            <div className="admin-header">
                <div>
                    <h2>Admin Dashboard</h2>
                    <p>Welcome, {user?.fullName}</p>
                </div>
                <button onClick={logout}>Logout</button>
            </div>

            {message && <p className="success">{message}</p>}
            {errorMessage && <p className="error">{errorMessage}</p>}

            {kpis && (
                <div className="kpi-grid">
                    <div className="kpi-card">
                        <h4>Total Active Users</h4>
                        <p>{kpis.totalUsers}</p>
                    </div>
                    <div className="kpi-card">
                        <h4>Students</h4>
                        <p>{kpis.totalStudents}</p>
                    </div>
                    <div className="kpi-card">
                        <h4>Professors</h4>
                        <p>{kpis.totalProfessors}</p>
                    </div>
                    <div className="kpi-card">
                        <h4>Admins</h4>
                        <p>{kpis.totalAdmins}</p>
                    </div>
                    <div className="kpi-card">
                        <h4>Pending Professors</h4>
                        <p>{kpis.totalPendingProfessors}</p>
                    </div>
                    <div className="kpi-card">
                        <h4>SmartSched Uses</h4>
                        <p>{kpis.totalSmartSchedRuns}</p>
                    </div>
                </div>
            )}

            <div className="admin-section">
                <h3>Pending Professor Registrations</h3>

                {pendingProfessors.length === 0 ? (
                    <p>No pending professor requests.</p>
                ) : (
                    <div className="compact-list">
                        {pendingProfessors.map((prof) => (
                            <div key={prof.id} className="compact-list-item">
                                <div>
                                    <strong>{prof.fullName}</strong>
                                    <div className="subtext">{prof.username}</div>
                                    <div className="subtext">{prof.email}</div>
                                </div>

                                <div className="action-group">
                                    <button onClick={() => approveProfessor(prof.id)}>Approve</button>
                                    <button className="danger-btn" onClick={() => declineProfessor(prof.id)}>
                                        Decline
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <div className="admin-section">
                <h3>All Users</h3>

                <div className="table-wrapper">
                    <table className="admin-table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Approved</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {users.map((u) => (
                                <tr key={u.id}>
                                    <td>{u.fullName}</td>
                                    <td>{u.email}</td>
                                    <td>{u.role}</td>
                                    <td>{u.isApproved ? "Yes" : "No"}</td>
                                    <td>
                                        <div className="table-actions">
                                            <select
                                                defaultValue=""
                                                onChange={(e) => {
                                                    if (e.target.value) {
                                                        changeRole(u.id, e.target.value);
                                                        e.target.value = "";
                                                    }
                                                }}
                                            >
                                                <option value="" disabled>
                                                    Change role
                                                </option>
                                                <option value="Student">Student</option>
                                                <option value="Professor">Professor</option>
                                                <option value="Admin">Admin</option>
                                            </select>

                                            <button
                                                className="danger-btn"
                                                onClick={() => confirmDeleteUser(u)}
                                            >
                                                Delete
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {userToDelete && (
                <div className="modal-overlay">
                    <div className="modal-box">
                        <h3>Confirm deletion</h3>
                        <p>
                            Are you sure you want to delete{" "}
                            <strong>{userToDelete.fullName}</strong>?
                        </p>
                        <div className="modal-actions">
                            <button onClick={cancelDelete}>Cancel</button>
                            <button className="danger-btn" onClick={deleteUser}>
                                Yes, Delete
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}