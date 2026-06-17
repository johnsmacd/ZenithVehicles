import { useEffect, useState } from "react";
import { Fine } from "../types/fine";
import { FineType } from "../enum/fineType";

const API_URL = "http://localhost:5200/api";

export function useFines(fineTypeFilter: string, 
  fineDateFilter: string | null,
  fineVehicleRegNoFilter: string) 
 {
  
  
  const [fines, setFines] = useState<Fine[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchFines = async () => {
      setLoading(true);
      setError(null);

      try {

        let queryElements: string[] = [];

        if (fineTypeFilter !== '') {
          const a = Object.keys(FineType).find((x) => {
            const key = x as keyof typeof FineType;
            return FineType[key] === parseInt(fineTypeFilter, 10);
          });
          queryElements.push(`finetype=${a}`);
        }

        if (fineDateFilter !== null) {
          queryElements.push(`finedate=${fineDateFilter}`);
        }

        if (fineVehicleRegNoFilter !== '') {
          queryElements.push(`vehicleregno=${encodeURIComponent(fineVehicleRegNoFilter)}`);
        }

        let queryString = queryElements.length > 0 ? `?${queryElements.join('&')}` : '';

        const response = await fetch(`${API_URL}/fines${queryString}`);

        if (!response.ok) {
          throw new Error(response.statusText);
        }

        const raw = await response.json();
        const fines = raw.map((fine: any) => ({
          ...fine,
          fineDate: new Date(fine.fineDate),
        }));

        setFines(fines);
      } catch (err) {
        console.error(err);
        setError("Failed to fetch fines");
      } finally {
        setLoading(false);
      }
    };

    fetchFines();
  }, [fineTypeFilter, fineDateFilter, fineVehicleRegNoFilter]);

  return { fines, loading, error };
}
