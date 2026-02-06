import { useState, useEffect } from "react";
import CalendarView from '../../components/CalendarView';

export default function StudentHome() {

    const userId = localStorage.getItem("userId");

    const [events, setEvents] = useState([]);
    const [selected, setSelected] = useState(null);

    const [form, setForm] = useState({
        title: "",
        start: "",
        end: "",
        description: ""
    });

    const [msg, setMsg] = useState("");

    useEffect(() => load(), []);

    const load = () => {
        fetch(`https://localhost:7243/api/events/my/${userId}`)
            .then(r => r.json())
            .then(setEvents);
    };

    const validate = () => {
        if (!form.title) return "Title required";
        if (!form.start || !form.end) return "Times required";
        if (form.end <= form.start)
            return "End must be after start";
        return "";
    };

    const save = async () => {

        const err = validate();
        if (err) { setMsg(err); return; }

        const url = selected
            ? `/api/events/edit/${selected.id}`
            : `/api/events/add`;

        const method = selected ? "PUT" : "POST";

        const res = await fetch(
            "https://localhost:7243" + url,
            {
                method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    ...form,
                    userId
                })
            });

        const text = await res.text();

        if (!res.ok) {
            setMsg(text);
            return;
        }

        setMsg("Saved!");
        setSelected(null);
        load();
    };

    const remove = async (id) => {
        await fetch(
            `https://localhost:7243/api/events/${id}`,
            { method: "DELETE" });
        load();
    };

    const select = (e) => {
        setSelected(e);
        setForm(e);
    };

    return (
        <div>

            <CalendarView
                events={events}
                onSelect={select} />

            <div className="card">

                <h4>{selected ? "Edit" : "Add"} Event</h4>

                <input
                    placeholder="Title"
                    value={form.title}
                    onChange={e => setForm({ ...form, title: e.target.value })} />

                <input type="datetime-local"
                    value={form.start}
                    onChange={e => setForm({ ...form, start: e.target.value })} />

                <input type="datetime-local"
                    value={form.end}
                    onChange={e => setForm({ ...form, end: e.target.value })} />

                <textarea
                    value={form.description}
                    onChange={e => setForm({ ...form, description: e.target.value })} />

                <button onClick={save}>Save</button>

                {selected &&
                    <button onClick={() => remove(selected.id)}>
                        Delete
                    </button>}

                {msg}
            </div>

        </div>
    );
}
