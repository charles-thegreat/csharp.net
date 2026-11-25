# About
This is a simple tool created as an educational exercise. It demonstrates the basic implementation of database models and REST API suites built using C# and ASP.NET. 

The app is basically a ticketing page. You can create a "Proposal", review/approve/deny it, and track how long it took to approve/deny. 

It is a simple illustration of the principle of Kaizen (改善, roughly “change for the better” in japanese) which Toyota heralds as a core idea in its manufacturing. 
The app is called Kaizen Tracker because it allows small, continuous improvements by following suggestions made by people doing the work. 

This tool could plausibly be upscaled in a real environment. A department could have its own tracker they'd use to improve their own processes: team members submit proposals, they get reviewed and approved, and the department tracks how long that process takes. The big metric on the page is "Lead Time" which represents that length of time. An initiative that could be taken with this tracker is reducing lead time, which is another Toyota measure. 

## Sample data

There's some sample data to illustrate what a board in-use would look like. Here are some of the terms listed in that data:

| Term | Meaning |
|---|---|
| **Kaizen** | Continuous improvement: small, frequent improvements suggested by the people doing the work |
| **Lead time** | Total elapsed time from request to delivery; here, from proposal submission to completion |
| **Andon** | A cord or button a worker uses to signal a problem and stop the line |
| **Poka-yoke** | Error-proofing: a jig or design that makes the mistake physically impossible |
| **Kanban** | A card or signal that triggers restocking parts only when they're used (just-in-time) |
| **Standard work** | The documented best way to do a task, which becomes the baseline for the next improvement |
| **Milk run** | A fixed delivery route that collects and drops off parts on a regular schedule |
| **Shadow board** | A tool board with outlines, so a missing tool is obvious at a glance |
