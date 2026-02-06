import { useEffect, useState } from "react";

export default function AdminHome() {

    const [users, setUsers] = useState([]);

    useEffect(() => load(), []);

    const load = () => {
        fetch("https://localhost:7243/api/admin/users")
            .then(r => r.json())
            .then(setUsers);
    };

    const change = async (id, role) => {
        await fetch(
            `https://localhost:7243/api/admin/role/${id}`,
            {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(role)
            });

        load();
    };

    return (
        <table>

            <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Role</th>
                <th></th>
            </tr>

            {users.map(u => (
                <tr>
                    <td>{u.fullName}</td>
                    <td>{u.email}</td>
                    <td>{u.role}</td>

                    <td>
                        <select
                            value={u.role}
                            onChange={e =>
                                change(u.id, e.target.value)}>

                            <option>Student</option>
                            <option>Employer</option>
                            <option>Admin</option>

                        </select>
                    </td>
                </tr>
            ))}
        </table>
    );
}
