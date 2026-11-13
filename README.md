# Paws & Claws Pet Store

Online pet store. Started 2009 as WebForms, moved to ASP.NET MVC 5 in 2013, "upgraded" to ASP.NET Core in 2021.

## Running

```
dotnet run --project PetStore.Web
```

Browse to http://localhost:5080. The SQLite database (`PetStore.Web/petstore.db`) is created and seeded on first run.
Delete the file to reset.

Test accounts: `admin` / `admin123`, `jdoe` / `password`, `msmith` / `password`.
Test cards: anything starting with 4 or 5. `4000000000000002` is declined.

## React cart

The cart page and mini cart are React (ClientApp/). The built bundle is committed in `wwwroot/Scripts/dist`
so you don't need node to run the site. To rebuild:

```
cd PetStore.Web/ClientApp
npm install
npm run build
```

The admin pet table uses in-browser babel (`wwwroot/Scripts/admin/petTable.jsx`), no build needed.

## Tests

```
dotnet test
```
