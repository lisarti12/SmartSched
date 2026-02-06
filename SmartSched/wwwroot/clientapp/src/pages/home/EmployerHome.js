import { useState, useEffect } from "react";

export default function EmployerHome({ name }) {

    const [events, setEvents] = useState([]);
    const [form, setForm] = useState({
        title: "",
        date: "",
        groupName: ""
    });

    const [msg, setMsg] = useState("");

    useEffect(() => load(), []);

    const load = () => {
        fetch("https://localhost:7243/api/events/group/all")
            .then(r => r.json())
            .then(setEvents);
    };

    const validate = () => {
        if (!form.title) return "Title required";
        if (!form.groupName) return "Group name required";
        if (!form.date) return "Date required";
        return "";
    };

    const add = async () => {

        const err = validate();
        if (err) { setMsg(err); return; }

        const res = await fetch("https://localhost:7243/api/events/group/add", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(form)
        });

        if (!res.ok) {
            setMsg(await res.text());
            return;
        }

        setMsg("Group event created");
        load();
    };

    return (
        <div>
            <h3>Employer Panel – {name}</h3>

            <div className="card">
                <input placeholder="Title"
                    onChange={e => setForm({ ...form, title: e.target.value })} />

                <input placeholder="Group"
                    onChange={e => setForm({ ...form, groupName: e.target.value })} />

                <input type="datetime-local"
                    onChange={e => setForm({ ...form, date: e.target.value })} />

                <button onClick={add}>Create</button>

                {msg && <div className="error">{msg}</div>}
            </div>

            {events.map(e => (
                <div className="card" key={e.id}>
                    {e.title} – {e.groupName}
                </div>
            ))}
        </div>
    );
}
